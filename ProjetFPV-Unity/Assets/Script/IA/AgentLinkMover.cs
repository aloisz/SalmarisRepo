using System;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class AgentLinkMover : MonoBehaviour
{
    public AnimationCurve m_Curve = new AnimationCurve();
    public float duration = 0.5f;
    public float tickVerification = 0.1f;
    public Quaternion linkerDirection;

    private NavMeshAgent agent;
    
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public IEnumerator StartLinkerVerif()
    {
        if (gameObject.activeSelf) yield return null;

        agent.autoTraverseOffMeshLink = false;
        while (true)
        {
            if (agent.isOnOffMeshLink)
            {
                yield return StartCoroutine(Curve(agent, duration));
                agent.CompleteOffMeshLink();
            }
            yield return new WaitForSeconds(tickVerification);
        }
    }

    IEnumerator Curve(NavMeshAgent agent, float duration)
    {
        OffMeshLinkData data = agent.currentOffMeshLinkData;
        Vector3 startPos = agent.transform.position;
        Vector3 endPos = data.endPos + Vector3.up * agent.baseOffset;

        Quaternion startRot = agent.transform.rotation;
        Vector3 direction = endPos - startPos;

        agent.enabled = false;
        
        float normalizedTime = 0.0f;
        while (normalizedTime < 1.0f)
        {
            float yOffset = m_Curve.Evaluate(normalizedTime);
            agent.transform.position = Vector3.Lerp(startPos, endPos, normalizedTime) + yOffset * Vector3.up;
            agent.transform.rotation = Quaternion.Slerp(startRot, Quaternion.LookRotation(direction), normalizedTime);
            normalizedTime += Time.deltaTime / duration;
            yield return null;
        }
        agent.enabled = true;
    }
}