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
    [SerializeField] private GameObject goAnchor = null;
    private Rigidbody anchorRb = null;
    private ConfigurableJoint anchorJoint = null;

    private GameObject grabBind = null;
    //[Space]
    //[SerializeField] private GameObject pickBind  = null;
    //[SerializeField] private Transform pickParentGo = null;
    //[SerializeField] private Vector3 pickPosition = Vector3.zero;
    //[Space]

    [SerializeField] private float minDistance = 0.5f;
    [SerializeField] private float maxDistance = 3;
    [SerializeField] private float pullForce = 300;
    //[SerializeField] private Vector3 pickGoMaxSize = new Vector3(1, 1, 1);
    //[SerializeField] private float grabGoMaxMass = 1;

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

                    grabBind.transform.localPosition = Vector3.forward * distanceToGo;

                    goAnchor.transform.position = hit.point;
                    anchorJoint.connectedBody = hitRb;

                    grabState = GrabState.grab;
                }
                else return;
            }
        }
    }

    private void Grab_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (grabState == GrabState.grab)
        {
            grabState = GrabState.free;
            hitGo = null;
            hitRb = null;
            anchorJoint.connectedBody = null;
        }
    }

    void Start()
    {

    }

    void FixedUpdate()
    {
        switch (grabState)
        {
            case GrabState.free:
                break;

            case GrabState.grab:
                if (Vector3.Distance(grabBind.transform.position, goAnchor.transform.position) > 0.01)
                    anchorRb.AddForce((grabBind.transform.position - goAnchor.transform.position) * pullForce, ForceMode.Force);
                    //anchorJoint.transform.position = Vector3.Lerp(anchorJoint.transform.position, grabBind.transform.position, Time.time * pullForce);
                break;
        }
    }

    private void GoCreator()
    {
        if (grabParentGo == null)
            grabParentGo = transform;

        grabBind = new GameObject("grabBind");
        grabBind.transform.parent = grabParentGo;

        goAnchor.transform.parent = null;
        anchorRb = goAnchor.GetComponent<Rigidbody>();
        anchorJoint = goAnchor.GetComponent<ConfigurableJoint>();
    }
}
