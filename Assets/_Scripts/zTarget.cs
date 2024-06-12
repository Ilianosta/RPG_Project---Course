using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class zTarget : MonoBehaviour
{
    public float viewScope;
    public Transform cam;
    public Transform target;
    public List<Transform> impacts = new List<Transform>();
    public List<Transform> targetsL = new List<Transform>();
    public List<Transform> targetsR = new List<Transform>();

    public Transform FirstTarget()
    {
        UpdateImpacts();

        target = impacts[0];
        return target;
    }

    private void UpdateImpacts()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, viewScope);
        impacts.Clear();

        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.gameObject.tag == "Target")
            {
                if (!impacts.Contains(hitCollider.transform)) impacts.Add(hitCollider.transform);
            }
        }
        impacts = impacts.OrderBy(i => Vector3.Distance(cam.position, i.position)).ToList();

        if (impacts.Count <= 0)
        {
            impacts.Add(null);
            target = impacts[0];
        }
    }

    public Transform NextToLeft()
    {
        UpdateImpacts();

        if (impacts.Count > 1)
        {
            targetsL.Clear();
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, viewScope);

            foreach (Collider hitCollider in hitColliders)
            {
                if (hitCollider.gameObject.tag == "Target")
                {
                    if (!targetsL.Contains(hitCollider.transform)) targetsL.Add(hitCollider.transform);
                }
            }

            targetsL = targetsL.OrderBy(i =>
            {
                Vector3 dir = (i.position - cam.position).normalized;
                float f = Vector3.Dot(dir, cam.right);
                return f;
            }).ToList();

            if (targetsL[0] == target)
            {
                MaxLeft();
            }
            else
            {
                Transform previous = null;

                foreach (Transform targetL in targetsL)
                {
                    if (targetL == target) break;
                    previous = targetL;
                }

                target = previous;
            }
        }

        return target;
    }

    public Transform NextToRight()
    {
        UpdateImpacts();

        if (impacts.Count > 1)
        {
            targetsR.Clear();
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, viewScope);

            foreach (Collider hitCollider in hitColliders)
            {
                if (hitCollider.gameObject.tag == "Target")
                {
                    if (!targetsR.Contains(hitCollider.transform)) targetsR.Add(hitCollider.transform);
                }
            }

            targetsR = targetsR.OrderByDescending(i =>
            {
                Vector3 dir = (i.position - cam.position).normalized;
                float f = Vector3.Dot(dir, cam.right);
                return f;
            }).ToList();

            if (targetsR[0] == target)
            {
                MaxRight();
            }
            else
            {
                Transform previous = null;

                foreach (Transform targetL in targetsR)
                {
                    if (targetL == target) break;
                    previous = targetL;
                }

                target = previous;
            }
        }

        return target;
    }

    public void MaxLeft()
    {
        targetsL.Clear();
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, viewScope);

        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.gameObject.tag == "Target")
            {
                if (!targetsL.Contains(hitCollider.transform)) targetsL.Add(hitCollider.transform);
            }
        }

        targetsL = targetsL.OrderBy(i =>
        {
            Vector3 dir = (i.position - cam.position).normalized;
            float f = Vector3.Dot(dir, cam.right);
            return f;
        }).ToList();

        targetsL.Remove(target);
        target = targetsL[0];
    }

    public void MaxRight()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, viewScope);

        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.gameObject.tag == "Target")
            {
                if (!targetsR.Contains(hitCollider.transform)) targetsR.Add(hitCollider.transform);
            }
        }

        targetsR = targetsR.OrderByDescending(i =>
        {
            Vector3 dir = (i.position - cam.position).normalized;
            float f = Vector3.Dot(dir, cam.right);
            return f;
        }).ToList();

        targetsR.Remove(target);
        target = targetsR[0];
    }
}
