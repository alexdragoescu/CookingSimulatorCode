using System;
using UnityEngine;

public class Player : MonoBehaviour, IKitchenObjectParent
{
    public static Player Instance { get; private set; }

    [SerializeField] private LayerMask countersLayerMask;
    private Vector3 lastInteractDirection;

    [SerializeField] private GameInput gameInput;
    [SerializeField] private Transform kitchenObjectHoldPoint;
    private ClearCounter selectedCounter;
    public event EventHandler<OnSelectedCounterChangedArgs> OnSelectedCounterChanged;
    private KitchenObject kitchenObject;

    public class OnSelectedCounterChangedArgs : EventArgs { public ClearCounter selectedCounter; }

    [SerializeField] private float movementSpeed = 7f;
    private bool isWalking;

    private void Awake()
    {
        if (Instance) Debug.LogError("There is more than one Player instance");
        Instance = this;
    }

    private void Start()
    {
        gameInput.OnInteractAction += GameInput_OnInteractAction;
    }

    private void GameInput_OnInteractAction(object sender, System.EventArgs e)
    {
        if (selectedCounter) selectedCounter.Interact(this);
    }

    private void Update()
    {
        HandleMovement();
        HandleInteraction();
    }

    public bool IsWalking()
    {
        return isWalking;
    }

    private void HandleInteraction()
    {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        Vector3 movementDirection = new Vector3(inputVector.x, 0f, inputVector.y);
        if (movementDirection != Vector3.zero) lastInteractDirection = movementDirection;

        float interactDistance = 2f;
        if (Physics.Raycast(transform.position, lastInteractDirection, out RaycastHit raycastHit, interactDistance, countersLayerMask))
            if (raycastHit.transform.TryGetComponent(out ClearCounter clearCounter))
            {
                if (clearCounter != selectedCounter)
                {
                    SetSelectedCounter(clearCounter);
                }
            }
            else SetSelectedCounter(null);
        else SetSelectedCounter(null);
        }

    private void HandleMovement()
    {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        Vector3 movementDirection = new Vector3(inputVector.x, 0f, inputVector.y);

        float movementDistance = movementSpeed * Time.deltaTime;
        float playerRadius = .7f;
        float playerHeight = 2f;
        bool canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, movementDirection, movementDistance);

        /* Cannot move towards movementDirection */
        if (!canMove)
        {
            /* X movement */
            Vector3 movementDirectionX = new Vector3(movementDirection.x, 0, 0).normalized;
            canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, movementDirectionX, movementDistance);

            if (canMove) movementDirection = movementDirectionX;
            else
            {
                /* Z movement */
                Vector3 movementDirectionZ = new Vector3(0, 0, movementDirection.z).normalized;
                canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, movementDirectionZ, movementDistance);

                if (canMove) movementDirection = movementDirectionZ;
            }
        }

        if (canMove) transform.position += movementDirection * movementDistance;

        isWalking = movementDirection != Vector3.zero;
        float rotationSpeed = 10f;
        transform.forward = Vector3.Slerp(transform.forward, movementDirection, Time.deltaTime * rotationSpeed);
    }

    private void SetSelectedCounter(ClearCounter selectedCounter)
    {
        this.selectedCounter = selectedCounter;
        OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedArgs
        {
            selectedCounter = selectedCounter
        });
    }

    public Transform GetKitchenObjectFollowTransform()
    {
        return kitchenObjectHoldPoint;
    }

    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        this.kitchenObject = kitchenObject;
    }

    public KitchenObject GetKitchenObject()
    {
        return kitchenObject;
    }

    public void ClearKitchenObject()
    {
        kitchenObject = null;
    }

    public bool HasKitchenObject()
    {
        return kitchenObject != null;
    }
}
