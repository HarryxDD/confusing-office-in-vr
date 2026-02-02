"""
LSL Data Analysis for ConfusingOffice VR Experiment
Analyzes grab, release, and placement events from Unity LSL streams
"""

import pyxdf
import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
from datetime import datetime
import os


class OfficeVRAnalyzer:
    def __init__(self, xdf_file_path):
        """Load and parse XDF file from LSL recording"""
        print(f"Loading {xdf_file_path}...")
        self.data, self.header = pyxdf.load_xdf(xdf_file_path)
        self.interaction_stream = None
        self.placement_stream = None
        self._parse_streams()
        
    def _parse_streams(self):
        """Identify and parse different LSL streams"""
        for stream in self.data:
            stream_name = stream['info']['name'][0]
            
            if 'Interactions' in stream_name:
                self.interaction_stream = self._parse_interaction_stream(stream)
                print(f"✓ Found interaction stream: {len(self.interaction_stream)} events")
                
            elif 'PlacementZones' in stream_name:
                self.placement_stream = self._parse_placement_stream(stream)
                print(f"✓ Found placement zone stream: {len(self.placement_stream)} events")
    
    def _parse_interaction_stream(self, stream):
        """Parse grab/release events"""
        events = []
        
        for i, timestamp in enumerate(stream['time_stamps']):
            event_str = stream['time_series'][i][0]
            parts = event_str.split('|')
            
            if len(parts) >= 7:  # Grab or Release event
                events.append({
                    'timestamp': timestamp,
                    'event_type': parts[0],
                    'object_name': parts[1],
                    'hand': parts[2],
                    'pos_x': float(parts[3]),
                    'pos_y': float(parts[4]),
                    'pos_z': float(parts[5]),
                    'placement_zone': parts[6] if len(parts) > 7 else 'N/A',
                    'unity_time': float(parts[-1])
                })
        
        return pd.DataFrame(events)
    
    def _parse_placement_stream(self, stream):
        """Parse placement zone enter/exit events"""
        events = []
        
        for i, timestamp in enumerate(stream['time_stamps']):
            event_str = stream['time_series'][i][0]
            parts = event_str.split('|')
            
            if len(parts) >= 8:
                events.append({
                    'timestamp': timestamp,
                    'event_type': parts[0],
                    'zone_name': parts[1],
                    'zone_type': parts[2],
                    'object_name': parts[3],
                    'pos_x': float(parts[4]),
                    'pos_y': float(parts[5]),
                    'pos_z': float(parts[6]),
                    'dwell_time': float(parts[7]) if parts[0] == 'Exit' else 0.0,
                    'unity_time': float(parts[-1])
                })
        
        return pd.DataFrame(events)
    
    def get_task_summary(self):
        """Calculate summary statistics"""
        if self.interaction_stream is None:
            return None
        
        grabs = self.interaction_stream[self.interaction_stream['event_type'] == 'Grab']
        releases = self.interaction_stream[self.interaction_stream['event_type'] == 'Release']
        
        summary = {
            'total_grabs': len(grabs),
            'total_releases': len(releases),
            'left_hand_grabs': len(grabs[grabs['hand'] == 'LeftHand']),
            'right_hand_grabs': len(grabs[grabs['hand'] == 'RightHand']),
            'unique_objects': grabs['object_name'].nunique(),
            'objects_grabbed': grabs['object_name'].value_counts().to_dict(),
        }
        
        # Placement analysis
        placed_in_zones = releases[releases['placement_zone'] != 'None']
        summary['successful_placements'] = len(placed_in_zones)
        summary['placement_zones_used'] = placed_in_zones['placement_zone'].value_counts().to_dict()
        
        return summary
    
    def calculate_interaction_times(self):
        """Calculate hold times (grab to release)"""
        if self.interaction_stream is None:
            return None
        
        grabs = self.interaction_stream[self.interaction_stream['event_type'] == 'Grab'].copy()
        releases = self.interaction_stream[self.interaction_stream['event_type'] == 'Release'].copy()
        
        hold_times = []
        
        for _, grab in grabs.iterrows():
            # Find matching release for same object
            matching_releases = releases[
                (releases['object_name'] == grab['object_name']) &
                (releases['timestamp'] > grab['timestamp'])
            ]
            
            if not matching_releases.empty:
                release = matching_releases.iloc[0]
                hold_time = release['timestamp'] - grab['timestamp']
                
                hold_times.append({
                    'object_name': grab['object_name'],
                    'grab_time': grab['timestamp'],
                    'release_time': release['timestamp'],
                    'hold_duration': hold_time,
                    'placed_in': release['placement_zone']
                })
        
        return pd.DataFrame(hold_times)
    
    def plot_timeline(self, save_path=None):
        """Visualize event timeline"""
        if self.interaction_stream is None:
            return
        
        fig, ax = plt.subplots(figsize=(14, 6))
        
        grabs = self.interaction_stream[self.interaction_stream['event_type'] == 'Grab']
        releases = self.interaction_stream[self.interaction_stream['event_type'] == 'Release']
        
        # Normalize timestamps to start from 0
        t0 = self.interaction_stream['timestamp'].min()
        
        ax.scatter(grabs['timestamp'] - t0, grabs['object_name'], 
                  marker='o', s=100, c='green', alpha=0.6, label='Grab')
        ax.scatter(releases['timestamp'] - t0, releases['object_name'], 
                  marker='x', s=100, c='red', alpha=0.6, label='Release')
        
        ax.set_xlabel('Time (seconds)')
        ax.set_ylabel('Object')
        ax.set_title('Object Interaction Timeline')
        ax.legend()
        ax.grid(True, alpha=0.3)
        
        plt.tight_layout()
        
        if save_path:
            plt.savefig(save_path, dpi=300)
            print(f"Timeline saved to {save_path}")
        else:
            plt.show()
    
    def export_to_csv(self, output_dir='processed_data'):
        """Export parsed data to CSV files"""
        os.makedirs(output_dir, exist_ok=True)
        
        if self.interaction_stream is not None:
            interactions_path = os.path.join(output_dir, 'interactions.csv')
            self.interaction_stream.to_csv(interactions_path, index=False)
            print(f"✓ Interactions exported to {interactions_path}")
        
        if self.placement_stream is not None:
            placement_path = os.path.join(output_dir, 'placements.csv')
            self.placement_stream.to_csv(placement_path, index=False)
            print(f"✓ Placements exported to {placement_path}")
        
        hold_times = self.calculate_interaction_times()
        if hold_times is not None:
            hold_path = os.path.join(output_dir, 'hold_times.csv')
            hold_times.to_csv(hold_path, index=False)
            print(f"✓ Hold times exported to {hold_path}")


def analyze_session(xdf_file):
    """Quick analysis of a recording session"""
    analyzer = OfficeVRAnalyzer(xdf_file)
    
    print("\n" + "="*50)
    print("SESSION SUMMARY")
    print("="*50)
    
    summary = analyzer.get_task_summary()
    if summary:
        print(f"Total Grabs: {summary['total_grabs']}")
        print(f"Total Releases: {summary['total_releases']}")
        print(f"Left Hand: {summary['left_hand_grabs']} | Right Hand: {summary['right_hand_grabs']}")
        print(f"Unique Objects: {summary['unique_objects']}")
        print(f"Successful Placements: {summary['successful_placements']}")
        
        print("\nObjects Grabbed:")
        for obj, count in summary['objects_grabbed'].items():
            print(f"  {obj}: {count} times")
        
        if summary['placement_zones_used']:
            print("\nPlacement Zones Used:")
            for zone, count in summary['placement_zones_used'].items():
                print(f"  {zone}: {count} objects")
    
    print("\n" + "="*50)
    
    # Export data
    analyzer.export_to_csv()
    
    # Generate timeline plot
    analyzer.plot_timeline('timeline.png')
    
    return analyzer


if __name__ == "__main__":
    # Example usage
    import sys
    
    if len(sys.argv) > 1:
        xdf_file = sys.argv[1]
    else:
        # Default file (replace with your recording)
        xdf_file = "recording.xdf"
    
    if os.path.exists(xdf_file):
        analyzer = analyze_session(xdf_file)
    else:
        print(f"File not found: {xdf_file}")
        print("\nUsage: python analyze_office_vr.py <recording.xdf>")
        print("\nMake sure to:")
        print("1. Install dependencies: pip install pyxdf pandas numpy matplotlib")
        print("2. Record data using LabRecorder while running your Unity scene")
        print("3. Run this script on the generated .xdf file")
