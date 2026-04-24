using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapSystem : MonoBehaviour
{
    public BoxCollider KnifeCollider;
    public Transform PlayerModel;
    public GameObject SelectionPrefab;

    private Transform SelectedObject1 = null;
    private Transform SelectedObject2 = null;
    private KnifeCount knifeCount = KnifeCount.Both;
    private InputReader ir;
    
    void Start()
    {
        ir = GetComponent<InputReader>();
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
            Debug.Log("no collider found");
            return knifeCount;
        }

        Collider closestCol = hits[0];
        float disFromOther = (transform.position - closestCol.transform.position).magnitude;

        foreach(Collider col in hits)
        {
            Debug.DrawRay(col.transform.position, Vector3.up*100, Color.red, 10);
            float disFromCol = (transform.position - col.transform.position).magnitude;

            if(disFromCol < disFromOther && col.transform.parent.parent.gameObject != gameObject)
            {
                closestCol = col;
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

                    var prefab = Instantiate(SelectionPrefab);
                    prefab.transform.parent = SelectedObject1;
                    prefab.transform.localPosition = new Vector3(0, 1.5f, 0);
                }
                else if(knifeCount == KnifeCount.One && SelectedObject1 != hit.collider.gameObject.transform)
                {
                    SelectedObject2 = hit.collider.gameObject.transform;
                    knifeCount = KnifeCount.Neither;

                    var prefab = Instantiate(SelectionPrefab);
                    prefab.transform.parent = SelectedObject2;
                    prefab.transform.localPosition = new Vector3(0, 1.5f, 0);
                }
            }
        }
        return knifeCount;
    }

    public KnifeCount SwapTrigger()
    {
        if(knifeCount == KnifeCount.One)
        {
            Swap(SelectedObject1, SelectedObject1, transform, PlayerModel);
            SelectedObject1 = null;
            SelectedObject2 = null;
        }
        else if(knifeCount == KnifeCount.Neither)
        {
            Swap(SelectedObject1, SelectedObject1, SelectedObject2, SelectedObject2);
            SelectedObject1 = null;
            SelectedObject2 = null;
        }
        return knifeCount;
    }

        void Swap(Transform Obj1, Transform Obj1Model, Transform Obj2, Transform Obj2Model)
    {
        if(Obj1 && Obj2)
        {
            Vector3 tempPos = Obj1.position;
            Quaternion tempRot = Obj1Model.rotation;
            Obj1.position = Obj2.position;
            Obj1Model.rotation = Obj2Model.rotation;
            Obj2.position = tempPos;
            Obj2Model.rotation = tempRot;
            if(Obj2 == transform)
            {
                GetComponent<PlayerMovementController>().setYaw(tempRot.y);
            }
            knifeCount = KnifeCount.Both;

            try {Destroy(Obj1.GetComponentInChildren<ParticleSystem>().gameObject);} catch {}
            try {Destroy(Obj2.GetComponentInChildren<ParticleSystem>().gameObject);} catch {}

            Obj1.GetComponent<ITeleportable>()?.OnTeleported();
            Obj2.GetComponent<ITeleportable>()?.OnTeleported();
        }
    }
}

public enum KnifeCount
{
    Both,
    One,
    Neither
}