using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapSystem : MonoBehaviour
{
    public BoxCollider KnifeCollider;
    public Transform PlayerModel;
    public Material SelectionMaterial;

    [Header("Hover Highlight")]
    public Material HoverMaterial;

    [Header("Aim Line")]
    public LineRenderer AimLine;
    public float AimLineLength = 10f;
    public Color AimLineDefaultColor = new Color(1f, 1f, 1f, 0.4f);
    public Color AimLineTargetColor = new Color(0.2f, 1f, 0.2f, 1f);

    [Header("Selection")]
    public float smallestKnifeHitRange;
    public float SelectionAngleBonus = 15f;

    private Transform SelectedObject1 = null;
    private Transform SelectedObject2 = null;
    private KnifeCount knifeCount = KnifeCount.Both;
    private InputReader ir;
    private HashSet<Renderer> _currentHovered = new HashSet<Renderer>();

    void Start()
    {
        ir = GetComponent<InputReader>();

        if (AimLine != null)
        {
            AimLine.positionCount = 2;
            AimLine.useWorldSpace = true;
        }
    }

    void Update()
    {
        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        Collider[] hits = Physics.OverlapBox(
            KnifeCollider.transform.position,
            KnifeCollider.size / 2,
            KnifeCollider.transform.rotation,
            LayerMask.GetMask("Swappable")
        );

        Transform closestValidTarget = null;
        float closestDist = float.MaxValue;

        foreach (Collider col in hits)
        {
            if (col.transform.root.gameObject == gameObject) continue;

            Vector3 toCol = col.transform.position - transform.position;
            float angle = Vector3.Angle(PlayerModel.forward, toCol);

            if (angle > smallestKnifeHitRange + SelectionAngleBonus) continue;

            float dist = toCol.magnitude;
            if (dist < closestDist)
            {
                closestDist = dist;
                closestValidTarget = col.transform;
            }
        }

        // Only highlight the single closest target, not everything in the cone
        HashSet<Renderer> newHovered = new HashSet<Renderer>();
        if (closestValidTarget != null)
        {
            Renderer rend = closestValidTarget.GetComponent<Renderer>() ?? closestValidTarget.GetComponentInParent<Renderer>();
            if (rend != null && !HasSelectionMaterial(rend))
                newHovered.Add(rend);
        }

        foreach (Renderer r in _currentHovered)
        {
            if (r != null && !newHovered.Contains(r))
                RemoveHoverMaterial(r);
        }
        foreach (Renderer r in newHovered)
        {
            if (r != null && !_currentHovered.Contains(r))
                AddHoverMaterial(r);
        }
        _currentHovered = newHovered;

        if (AimLine != null)
        {
            Vector3 origin = PlayerModel.position;
            AimLine.SetPosition(0, origin);

            if (closestValidTarget != null)
            {
                AimLine.SetPosition(1, closestValidTarget.position);
                AimLine.startColor = AimLineTargetColor;
                AimLine.endColor = AimLineTargetColor;
            }
            else
            {
                AimLine.SetPosition(1, origin + PlayerModel.forward * AimLineLength);
                AimLine.startColor = AimLineDefaultColor;
                AimLine.endColor = AimLineDefaultColor;
            }
        }
    }

    bool HasSelectionMaterial(Renderer rend)
    {
        foreach (Material m in rend.materials)
            if (m.name.Contains(SelectionMaterial.name)) return true;
        return false;
    }

    void AddHoverMaterial(Renderer rend)
    {
        if (HoverMaterial == null) return;
        var mats = new List<Material>(rend.materials);
        if (!mats.Exists(m => m.name.Contains(HoverMaterial.name)))
        {
            mats.Add(HoverMaterial);
            rend.materials = mats.ToArray();
        }
    }

    void RemoveHoverMaterial(Renderer rend)
    {
        if (HoverMaterial == null) return;
        var mats = new List<Material>(rend.materials);
        mats.RemoveAll(m => m.name.Contains(HoverMaterial.name));
        rend.materials = mats.ToArray();
    }

    public KnifeCount SwapHit()
    {
        Collider[] hits = Physics.OverlapBox
        (
            KnifeCollider.transform.position,
            KnifeCollider.size / 2,
            KnifeCollider.transform.rotation,
            LayerMask.GetMask("Swappable")
        );

        if(hits.Length == 0)
        {
            Debug.Log("No Collider Found.");
            return knifeCount;
        }

        Collider closestCol = hits[0];
        float disFromOther = (transform.position - closestCol.transform.position).magnitude;

        foreach(Collider col in hits)
        {
            Debug.DrawRay(col.transform.position, Vector3.up*100, Color.red, 10);
            float disFromCol = (transform.position - col.transform.position).magnitude;

            bool closest = disFromCol < disFromOther;
            bool playerObject = col.transform.root.gameObject == gameObject;
            Vector3 playerToCol = col.transform.position - transform.position;
            float angle = Vector3.Angle(PlayerModel.transform.forward, playerToCol);

            float angleLimit = smallestKnifeHitRange + SelectionAngleBonus;

            bool outOfRange = Mathf.Abs(angle) > angleLimit;
            Debug.Log(angle);
            Debug.DrawLine(transform.position, transform.position + PlayerModel.forward, Color.black, 10f);

            if(closest && !playerObject && !outOfRange)
            {
                closestCol = col;
                disFromOther = disFromCol;
            }
        }

        Ray KnifeRay = new Ray(transform.position, PlayerModel.forward);
        RaycastHit hit;
        Vector3 dir = (closestCol.transform.position - KnifeRay.origin).normalized;
        Debug.DrawRay(KnifeRay.origin, dir, Color.red);
        if(Physics.Raycast(KnifeRay.origin, dir, out hit, 20f, LayerMask.GetMask("Swappable")))
        {
            if(hit.collider == closestCol) {
                if(knifeCount == KnifeCount.Both)
                {
                    SelectedObject1 = hit.collider.gameObject.transform;
                    SelectedObject2 = transform;
                    knifeCount = KnifeCount.One;

                    Renderer rend = SelectedObject1.GetComponent<Renderer>();
                    RemoveHoverMaterial(rend);
                    Material[] mats = rend.materials;
                    Array.Resize(ref mats, mats.Length + 1);
                    mats[mats.Length - 1] = SelectionMaterial;
                    rend.materials = mats;
                }
                else if(knifeCount == KnifeCount.One && SelectedObject1 != hit.collider.gameObject.transform)
                {
                    SelectedObject2 = hit.collider.gameObject.transform;
                    knifeCount = KnifeCount.Neither;

                    Renderer rend = SelectedObject2.GetComponent<Renderer>();
                    RemoveHoverMaterial(rend);
                    Material[] mats = rend.materials;
                    Array.Resize(ref mats, mats.Length + 1);
                    mats[mats.Length - 1] = SelectionMaterial;
                    rend.materials = mats;
                }
            }
        }
        return knifeCount;
    }

    public KnifeCount SwapTrigger()
    {
        if(knifeCount == KnifeCount.One)
        {
            Swap(SelectedObject1);
            SelectedObject1 = null;
            SelectedObject2 = null;
        }
        else if(knifeCount == KnifeCount.Neither)
        {
            Swap(SelectedObject1, SelectedObject2);
            SelectedObject1 = null;
            SelectedObject2 = null;
        }
        return knifeCount;
    }

    void Swap(Transform Obj1, Transform Obj2)
    {
        if(Obj1 && Obj2)
        {
            Vector3 tempPos = Obj1.position;
            Quaternion tempRot = Obj1.rotation;
            Obj1.position = Obj2.position;
            Obj1.rotation = Obj2.rotation;
            Obj2.position = tempPos;
            Obj2.rotation = tempRot;
            if(Obj2 == transform)
            {
                GetComponent<PlayerMovementController>().setYaw(tempRot.y);
            }
            knifeCount = KnifeCount.Both;

            Renderer rend = Obj1.GetComponent<Renderer>();
            var mats = new List<Material>(rend.materials);
            mats.RemoveAll(m => m.name.Contains(SelectionMaterial.name));
            rend.materials = mats.ToArray();

            rend = Obj2.GetComponent<Renderer>();
            mats = new List<Material>(rend.materials);
            mats.RemoveAll(m => m.name.Contains(SelectionMaterial.name));
            rend.materials = mats.ToArray();

            Obj1.GetComponent<ITeleportable>()?.OnTeleported();
            Obj2.GetComponent<ITeleportable>()?.OnTeleported();
        }
    }

    void Swap(Transform Obj1)
    {
        if(Obj1 && transform)
        {
            Vector3 tempPos = Obj1.position;
            Quaternion tempRot = Obj1.rotation;
            Obj1.position = transform.position;
            Obj1.rotation = PlayerModel.rotation;
            transform.position = tempPos;
            PlayerModel.rotation = tempRot;
            if(transform == transform)
            {
                GetComponent<PlayerMovementController>().setYaw(tempRot.y);
            }
            knifeCount = KnifeCount.Both;

            Renderer rend = Obj1.GetComponent<Renderer>();
            var mats = new List<Material>(rend.materials);
            mats.RemoveAll(m => m.name.Contains(SelectionMaterial.name));
            rend.materials = mats.ToArray();

            Obj1.GetComponent<ITeleportable>()?.OnTeleported();
            transform.GetComponent<ITeleportable>()?.OnTeleported();
        }
    }
}

public enum KnifeCount
{
    Both,
    One,
    Neither
}
