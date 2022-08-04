using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PullandPush : MonoBehaviour
{
    private InputMaster controller;
    private enum GrabState {free, grab, pick}
    [SerializeField, ReadOnly] private GrabState grabState = GrabState.free;

    [Header("Preferences")]
    [SerializeField] private Transform grabParentGo = null;
    //[SerializeField] private bool fixedPosition     = false;
    //[SerializeField] private Vector3 grabPosition   = Vector3.zero;
    private GameObject grabBind = null;
    private GameObject goAnchor = null;
    private Rigidbody  anchorRb = null;
    private ConfigurableJoint anchorJoint = null;

    [Space]
    [SerializeField] private GameObject pickBind  = null;
    [SerializeField] private Transform pickParentGo = null;
    [SerializeField] private Vector3 pickPosition = Vector3.zero;
    [Space]
    [SerializeField] private float minDistance = 0.5f;
    [SerializeField] private float maxDistance = 3;
    [SerializeField] private float force = 5;
    [SerializeField] private Vector3 pickGoMaxSize = new Vector3(1, 1, 1);
    [SerializeField] private float grabGoMaxMass = 1;

    private Transform hitGo = null;
    private Rigidbody hitRb = null;


    private void OnEnable()
    {
        controller.Enable();
    }

    private void OnDisable()
    {
        controller.Disable();
    }

    private void Awake()
    {
        controller = new InputMaster();

        controller.Player.Grab.performed += Grab_performed;
        controller.Player.Grab.canceled  += Grab_canceled;

        GoCreator();
    }

    private void Grab_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        GrabRaycaster();
    }

    private void Grab_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        grabState = GrabState.free;
        hitRb.useGravity = true;
        hitRb = null;
        anchorJoint.connectedBody = null;
    }

    void Start()
    {
        grabBind.transform.parent = grabParentGo;
    }

    void FixedUpdate()
    {
        switch (grabState)
        {
            case GrabState.free:
                break;

            case GrabState.grab:
                if (Vector3.Distance(grabBind.transform.position, goAnchor.transform.position) > 0.1)
                    goAnchor.GetComponent<Rigidbody>().AddForce((grabBind.transform.position - goAnchor.transform.position) * force, ForceMode.Acceleration);
                
                break;
        }
    }

    private void GrabRaycaster()
    {
        if (grabState == GrabState.free)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, maxDistance))
            {
                if (hit.transform.GetComponent<Rigidbody>())
                {
                    hitGo = hit.transform;
                    hitRb = hit.rigidbody;

                    float distanceToGo = (transform.position - hit.point).magnitude;
                    if (distanceToGo < minDistance) distanceToGo = minDistance;
                    grabBind.transform.localPosition = Vector3.zero;
                    grabBind.transform.localPosition = new Vector3(grabBind.transform.localPosition.x, grabBind.transform.localPosition.y, distanceToGo);
                    //Debug.Log(Vector3.Scale(hitGo.GetComponent<MeshFilter>().mesh.bounds.size, hitGo.transform.lossyScale));

                    goAnchor.transform.position = hit.point;
                    anchorJoint.connectedBody = hitRb;

                    grabState = GrabState.grab;
                }
            }
        }
    }

    private void GoCreator()
    {
        goAnchor = new GameObject("goAnchor");
        anchorJoint = goAnchor.AddComponent<ConfigurableJoint>();
        anchorJoint.xMotion = ConfigurableJointMotion.Locked;
        anchorJoint.yMotion = ConfigurableJointMotion.Locked;
        anchorJoint.zMotion = ConfigurableJointMotion.Locked;
        anchorJoint.axis = Vector3.zero;
        anchorJoint.autoConfigureConnectedAnchor = true;
        goAnchor.GetComponent<Rigidbody>().useGravity = false;
        goAnchor.GetComponent<Rigidbody>().drag = 10;
        goAnchor.GetComponent<Rigidbody>().angularDrag = 10;
        
        grabBind = new GameObject("grabAnchor");
        
        if (grabParentGo == null)
            grabParentGo = transform;
    }
}
