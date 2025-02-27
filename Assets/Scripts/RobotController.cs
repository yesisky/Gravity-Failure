using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RobotController : MonoBehaviour
{
    //parameters
    public float kickForce = 200f;
    public float jetpackForce = 2f;
    public float propellerForce = 2f;
    public bool usingJetpack = false;
    public bool propelling = false;
    public ParticleSystem jetpackParticles;
    public ParticleSystem propellerParticles;
    public GameObject arms;
    public Animator animator;

    //info
    public Vector3 targetPointed = Vector3.zero;
    public Vector2 inputPosition = Vector2.zero;

    //internal
    private Rigidbody rb;
    private PropellerFuel propellerFuel;
    private RobotBateries robotBateries;

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        propellerFuel = gameObject.GetComponent<PropellerFuel>();
        robotBateries = gameObject.GetComponent<RobotBateries>();


        jetpackParticles.Stop();
        propellerParticles.Stop();
    }

    private void FixedUpdate()
    {
        UseJetpack();
        Propel();
    }

    private void Update()
    {
        CalculateTargetPointed();

        arms.transform.LookAt(new Vector3(targetPointed.x, 5, targetPointed.z));
    }

    private void CalculateTargetPointed()
    {
        Ray ray = Camera.main.ScreenPointToRay(inputPosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100, 1 << LayerMask.NameToLayer("Floor")))
        {
            targetPointed = hit.point;
        }
    }

    private void Kick()
    {
        if (!robotBateries.IsEmpty())
        {
            //Debug.Log("use kick " + (direction == 1 ? "left" : "right"));

            animator.SetTrigger("KickRight");

            Collider[] propsHitted = Physics.OverlapSphere(transform.position + (-1) * transform.up * 0.2f, 1.25f, 1 << LayerMask.NameToLayer("Spaceship"));
            if (propsHitted.Length > 0)
            {
                rb.AddForce((-1) * (transform.position - targetPointed).normalized * kickForce);

                foreach (Collider propHitted in propsHitted)
                {
                    Debug.Log("kick collides on " + propHitted);
                    propHitted.GetComponent<Rigidbody>().AddForce((transform.position - targetPointed).normalized * kickForce);
                }
            }
        }
    }

    private void UseJetpack()
    {
        if (usingJetpack && !robotBateries.IsEmpty())
        {
            rb.AddForce(transform.up * jetpackForce);
        }
        else
        {
            jetpackParticles.Stop();
        }
    }

    private void Propel()
    {
        if (propelling && !propellerFuel.IsEmpty() && !robotBateries.IsEmpty())
        {
            rb.AddForce((transform.position - targetPointed) * propellerForce);
            propellerParticles.transform.LookAt(targetPointed);
            propellerFuel.Consume();
        }
        else
        {
            propellerParticles.Stop();
        }
    }

    public void OnAim(InputAction.CallbackContext context)
    {
        inputPosition = context.ReadValue<Vector2>();
    }

    public void OnKick(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Kick();
        }
    }

    public void OnLeftKick(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Kick();
        }
    }

    public void OnRightKick(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Kick();
        }
    }

    public void OnJetpack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            UseJetpack();
        }
        switch (context.phase)
        {
            case InputActionPhase.Started:
                Debug.Log("Start Jetpack");
                usingJetpack = true;
                if (!robotBateries.IsEmpty()) jetpackParticles.Play();
                break;
            case InputActionPhase.Canceled:
                Debug.Log("Stop Jetpack");
                usingJetpack = false;
                jetpackParticles.Stop();
                break;
        }
    }

    public void OnPropel(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started:
                Debug.Log("Start Propelling");
                propelling = true;
                if (!propellerFuel.IsEmpty() && !robotBateries.IsEmpty()) propellerParticles.Play();
                break;
            case InputActionPhase.Canceled:
                Debug.Log("Stop Propelling");
                propelling = false;
                propellerParticles.Stop();
                break;
        }
    }

    private void OnDrawGizmos()
    {
        //Gizmos.color = Color.red;
        //if (Physics.OverlapSphere(transform.position + transform.right * (-1), 0.75f, 1 << LayerMask.NameToLayer("Spaceship")).Length > 0)
        //    Gizmos.DrawSphere(transform.position + transform.right * (-1), 0.75f);
        //else
        //    Gizmos.DrawWireSphere(transform.position + transform.right * (-1), 0.75f);

        //Gizmos.color = Color.blue;
        //if (Physics.OverlapSphere(transform.position + transform.right * 1, 0.75f, 1 << LayerMask.NameToLayer("Spaceship")).Length > 0)
        //    Gizmos.DrawSphere(transform.position + transform.right, 0.75f);
        //else
        //    Gizmos.DrawWireSphere(transform.position + transform.right, 0.75f);

        Gizmos.color = Color.green;
        if (Physics.OverlapSphere(transform.position + (-1) * transform.up * 0.2f, 1.25f, 1 << LayerMask.NameToLayer("Spaceship")).Length > 0)
            Gizmos.DrawSphere(transform.position + (-1) * transform.up * 0.2f, 1.25f);
        else
            Gizmos.DrawWireSphere(transform.position + (-1) * transform.up * 0.2f, 1.25f);

        if (targetPointed != Vector3.zero)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, targetPointed);
        }
    }
}
