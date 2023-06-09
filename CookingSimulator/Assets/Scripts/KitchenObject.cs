using UnityEngine;
using Unity.Netcode;

public class KitchenObject : NetworkBehaviour
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;
    private FollowTransform followTransform;

    private IKitchenObjectParent kitchenObjectParent;

    protected virtual void Awake()
    {
        followTransform = GetComponent<FollowTransform>();
    }

    public KitchenObjectSO GetKitchenObjectSO()
    {
        return kitchenObjectSO;
    }

    public void SetKitchenObjectParent(IKitchenObjectParent kitchenObjectParent)
    {
        SetKitchenObjectParentServerRpc(kitchenObjectParent.GetNetworkObject());
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetKitchenObjectParentServerRpc(NetworkObjectReference kitchenObjectParentNetworkObjectReference)
    {
        SetKitchenObjectParentClientRpc(kitchenObjectParentNetworkObjectReference);
    }

    [ClientRpc]
    private void SetKitchenObjectParentClientRpc(NetworkObjectReference kitchenObjectParentNetworkObjectReference)
    {
        kitchenObjectParentNetworkObjectReference.TryGet(out NetworkObject kitchenObjectParentNetworkObject);
        IKitchenObjectParent kitchenObjectParent = kitchenObjectParentNetworkObject.GetComponent<IKitchenObjectParent>();

        if (this.kitchenObjectParent != null) this.kitchenObjectParent.ClearKitchenObject();
        this.kitchenObjectParent = kitchenObjectParent;
        if (kitchenObjectParent.HasKitchenObject()) Debug.LogError("KitchenObjectParent already has a KitchenObject");
        kitchenObjectParent.SetKitchenObject(this);

        followTransform.SetTargetTransform(kitchenObjectParent.GetKitchenObjectFollowTransform());
    }

    public IKitchenObjectParent GetKitchenObjectParent()
    {
        return kitchenObjectParent;
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }

    public void ClearKitchenObjectOnParent()
    {
        kitchenObjectParent.ClearKitchenObject();
    }

    public static void SpawnKitchenObject(KitchenObjectSO kitchenObjectSO, IKitchenObjectParent kitchenObjectParent)
    {
        KitchenGameMultiplayer.Instance.SpawnKitchenObject(kitchenObjectSO, kitchenObjectParent);
    }
    
    public static void DestroyKitchenObject(KitchenObject kitchenObject)
    {
        KitchenGameMultiplayer.Instance.DestroyKitchenObject(kitchenObject);
    }

    public bool TryGettingPlate(out PlateKitchenObject plateKitchenObject)
    {
        if (this is PlateKitchenObject)
        {
            plateKitchenObject = this as PlateKitchenObject;
            return true;
        }

        plateKitchenObject = null;
        return false;
    }
}
