using System.Collections;
using UnityEngine;
using UnityJapanOffice;





namespace UnityJapanOffice {
	
	[RequireComponent(typeof(Rigidbody))]

	public class WalkThrough : MonoBehaviour {

		[SerializeField] private Transform player;
		[SerializeField] private Transform cam;

		[Space(10)]
		[SerializeField] private  float moveSpeed = 1;
        [SerializeField] private float runningSpeedFactor = 3;
        [SerializeField] private  float rotateSpeed = 1;

	
		private Rigidbody rigid;
		private Vector3 dir = Vector3.zero;
		private float yaw = 0, tilt = 0;

		private bool isRunning = false;
        private bool isRunningToggle = false;
        private bool isLockedPosition = false;
		private bool isLockedRotation = false;




		private void Start () {
			rigid = GetComponent<Rigidbody>();
		}



		private void Update() {
			if(!isLockedRotation){
				var lookVec = InputWrapper.GetLookDelta();
				yaw  += lookVec.x * rotateSpeed;
				tilt -= lookVec.y * rotateSpeed;

				tilt = Mathf.Clamp(tilt, -89, 89);

				player.eulerAngles = new Vector3(0, yaw, 0);
				cam.localEulerAngles = new Vector3(tilt, 0, 0);
			}


			if(!isLockedPosition){
				isRunning = false;
                if (InputWrapper.IsTriggerToggleSprint()) {
                    isRunningToggle = !isRunningToggle;
				}
				if (InputWrapper.IsSprint())
				{
					isRunningToggle = false;
                    isRunning = true;
                }
                isRunning |= isRunningToggle;

                var moveVec = InputWrapper.GetMoveVector();
				dir = Quaternion.Euler(0, player.localEulerAngles.y, 0) * new Vector3(moveVec.x, 0 , moveVec.y);

				if(isRunning) {
					dir *= moveSpeed * runningSpeedFactor;
				} else {
					dir *= moveSpeed;
				}
				
				rigid.linearVelocity = dir * Time.deltaTime;
			}
			else
			{
				rigid.linearVelocity = Vector3.zero;
			}
		}



		public void Teleport (Transform target) {
			player.position = target.position;
			player.rotation = target.rotation;

			yaw = player.eulerAngles.y;
		}



		public void LockPosition (bool value) {
			isLockedPosition = value;
		}



		public void LockRotation (bool value) {
			isLockedRotation = value;

			if(value){
				Cursor.visible = true;
				Cursor.lockState = CursorLockMode.None;
			} else {
				Cursor.visible = false;
				Cursor.lockState = CursorLockMode.Locked;
			}
		}
	}
}