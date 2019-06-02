using UnityEngine;
using System.Collections.Generic;
namespace Complete
{
    public class FlowCortrol : MonoBehaviour
    {   
        public float m_DampTime = 0.2f;
        public float m_ScreenEdgeBuffer = 4f;
        public float m_MinSize = 6.5f;
        // [HideInInspector] public Transform[] m_Targets;
        [HideInInspector] public List<Transform> m_Targets;
        private Camera m_Camera;
        private float m_ZoomSpeed;
        private Vector3 m_MoveVelocity;
        private Vector3 m_DesiredPosition; 

        private void Awake ()
        {
            m_Targets = new List<Transform>();
            m_Camera = GetComponentInChildren<Camera> ();
        }
        public void AddTarget(Transform target)
        {
            m_Targets.Add(target);
            Debug.Log("   m_Targets.Count = " + m_Targets.Count);
            Debug.Log(" FlowCortrol ---------- AddTarge-------" + target  + " " + target.position );
        }
        private void FixedUpdate ()
        {
            Move ();
            Zoom ();
        }
        private void Move()
        {
            FindAveragePosition ();
            transform.position = Vector3.SmoothDamp(transform.position, m_DesiredPosition, ref m_MoveVelocity, m_DampTime);
        }
        private void Zoom ()
        {
            float requiredSize = FindRequiredSize();
            m_Camera.orthographicSize = Mathf.SmoothDamp (m_Camera.orthographicSize, requiredSize, ref m_ZoomSpeed, m_DampTime);
        }
        private void FindAveragePosition ()
        {
            Vector3 averagePos = new Vector3 ();
            int numTargets = 0;
            foreach (Transform element in m_Targets)
            {
                if (element == null||!element.gameObject.activeSelf)
                    continue;
                averagePos += element.position;
                numTargets++;
            }

            if (numTargets > 0)
                averagePos /= numTargets;
            averagePos.y = transform.position.y;
            m_DesiredPosition = averagePos;
        }
        private float FindRequiredSize ()
        {
            Vector3 desiredLocalPos = transform.InverseTransformPoint(m_DesiredPosition);
            float size = 0f;
            foreach (Transform element in m_Targets)
            {
                if (element == null||!element.gameObject.activeSelf)
                    continue;
                Vector3 targetLocalPos = transform.InverseTransformPoint(element.position);
                Vector3 desiredPosToTarget = targetLocalPos - desiredLocalPos;
                size = Mathf.Max(size, Mathf.Abs(desiredPosToTarget.y));
                size = Mathf.Max(size, Mathf.Abs(desiredPosToTarget.x) / m_Camera.aspect);
            }
            size += m_ScreenEdgeBuffer;
            size = Mathf.Max (size, m_MinSize);
            return size;
        }
        public void SetStartPositionAndSize ()
        {
            FindAveragePosition ();
            transform.position = m_DesiredPosition;
            m_Camera.orthographicSize = FindRequiredSize ();
        }
    }
}