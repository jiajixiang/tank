using UnityEngine;

namespace Complete
{
    public class TankMovement : MonoBehaviour
    {
        public int m_PlayerNumber = 1;              // Used to identify which tank belongs to which player.  This is set by this tank's manager.
        public float m_Speed = 10f;                 // How fast the tank moves forward and back.
        public float m_TurnSpeed = 1.0f;            // How fast the tank turns in degrees per second.
        public AudioSource m_MovementAudio;         // Reference to the audio source used to play engine sounds. NB: different to the shooting audio source.
        public AudioClip m_EngineIdling;            // Audio to play when the tank isn't moving.
        public AudioClip m_EngineDriving;           // Audio to play when the tank is moving.
		public float m_PitchRange = 0.2f;           // The amount by which the pitch of the engine noises can vary.

        private string m_MovementAxisName;          // The name of the input axis for moving forward and back.
        private string m_TurnAxisName;              // The name of the input axis for turning.
        private Rigidbody m_Rigidbody;              // Reference used to move the tank.
        private float m_MovementInputValue;         // The current value of the movement input.
        private float m_TurnInputValue;             // The current value of the turn input.
        private float m_OriginalPitch;              // The pitch of the audio source at the start of the scene.
        private ParticleSystem[] m_particleSystems; // References to all the particles systems used by the Tanks
        private float inputHorizontal;
        private float inputVertical, angle, angle1, angle2;
        private Vector3 m_TurnValue;
        private float dir;
        private Vector3 m_MousePositionLast;//用于计算宣布在方向的起始位置
        private Vector3 m_MousePositionNew;//用于计算移动方向的结尾位置
        private Vector3 m_MouseMoveDirection;//用于表示旋转方向
        private void Awake ()
        {
            m_Rigidbody = GetComponent<Rigidbody> ();
        }


        private void OnEnable ()
        {
            // When the tank is turned on, make sure it's not kinematic.
            m_Rigidbody.isKinematic = false;

            // Also reset the input values.
            m_MovementInputValue = 0f;
            m_TurnInputValue = 0f;

            // We grab all the Particle systems child of that Tank to be able to Stop/Play them on Deactivate/Activate
            // It is needed because we move the Tank when spawning it, and if the Particle System is playing while we do that
            // it "think" it move from (0,0,0) to the spawn point, creating a huge trail of smoke
            m_particleSystems = GetComponentsInChildren<ParticleSystem>();
            for (int i = 0; i < m_particleSystems.Length; ++i)
            {
                m_particleSystems[i].Play();
            }
        }


        private void OnDisable ()
        {
            // When the tank is turned off, set it to kinematic so it stops moving.
            m_Rigidbody.isKinematic = true;

            // Stop all particle system so it "reset" it's position to the actual one instead of thinking we moved when spawning
            for(int i = 0; i < m_particleSystems.Length; ++i)
            {
                m_particleSystems[i].Stop();
            }
        }


        private void Start ()
        {
            // The axes names are based on player number.
            m_MovementAxisName = "Vertical";
            m_TurnAxisName = "Horizontal";

            // Store the original pitch of the audio source.
            m_OriginalPitch = m_MovementAudio.pitch;
        }


        private void Update ()
        {
            // Store the value of both input axes.
            m_MovementInputValue = SimpleInput.GetAxis(m_MovementAxisName);
            m_TurnInputValue = SimpleInput.GetAxis(m_TurnAxisName);
            EngineAudio ();
        }


        private void EngineAudio ()
        {
            // If there is no input (the tank is stationary)...
            if (Mathf.Abs (m_MovementInputValue) < 0.1f && Mathf.Abs (m_TurnInputValue) < 0.1f)
            {
                // ... and if the audio source is currently playing the driving clip...
                if (m_MovementAudio.clip == m_EngineDriving)
                {
                    // ... change the clip to idling and play it.
                    m_MovementAudio.clip = m_EngineIdling;
                    m_MovementAudio.pitch = Random.Range (m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
                    m_MovementAudio.Play ();
                }
            }
            else
            {
                // Otherwise if the tank is moving and if the idling clip is currently playing...
                if (m_MovementAudio.clip == m_EngineIdling)
                {
                    // ... change the clip to driving and play.
                    m_MovementAudio.clip = m_EngineDriving;
                    m_MovementAudio.pitch = Random.Range(m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
                    m_MovementAudio.Play();
                }
            }
        }

        private void FixedUpdate ()
        {
            Move();
            if (m_MovementInputValue != 0f)
                m_MovementInputValue = m_MovementInputValue > 0f ? 1.0f : -1.0f;
            if (m_TurnInputValue != 0f)
                m_TurnInputValue = m_TurnInputValue > 0f ? -1.0f : 1.0f;
            Turn();
        }

        private void Move ()
        {
            Vector3 m_MovementValue = new Vector3(m_MovementInputValue, 0, m_TurnInputValue);
            float m_Value = Vector3.Magnitude(m_MovementValue);
            m_Value = m_Value > 1.0f ? 1.0f : m_Value;
            Vector3 movement = transform.forward * m_Value * m_Speed * Time.deltaTime;
            m_Rigidbody.MovePosition(m_Rigidbody.position + movement);
        }
        
        private void Turn()
        {
            m_TurnValue = new Vector3(m_MovementInputValue, 0f, -1.0f * m_TurnInputValue);
            angle = Vector3.Angle(transform.forward, m_TurnValue);
            if(angle != 0f)
            {
                float mydir = (Vector3.Dot(Vector3.up, Vector3.Cross(m_TurnValue, transform.forward)) < 0 ? 1.0f : -1.0f);
                float turn = angle * mydir * Time.deltaTime;
                transform.Rotate(0, turn, 0);
                /*
                if(dir == null)
                    dir = mydir;
                if (mydir != dir){
                    angle *= dir;
                    dir = mydir;
                Quaternion turnRotation = Quaternion.Euler(0f, angle, 0f);
                m_Rigidbody.MoveRotation(m_Rigidbody.rotation * turnRotation);
                if (angle < m_TurnSpeed)
                    transform.forward = m_TurnValue;
                else
                }*/
            }
        }
        private float angle_360(Vector3 from_, Vector3 to_)
        {
            Vector3 v3 = Vector3.Cross(from_, to_);
            if (v3.z < 0f)
                return Vector3.Angle(from_, to_);
            else
                return 360 - Vector3.Angle(from_, to_);
        }
    }
}