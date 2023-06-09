using UnityEngine;

public class ClearCounter : BaseCounter
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;

    public override void Interact(Player player)
    {
        if (!HasKitchenObject())
        {
            if (player.HasKitchenObject()) player.GetKitchenObject().SetKitchenObjectParent(this);
        }
        else if (player.HasKitchenObject())
        {
            if (player.GetKitchenObject().TryGettingPlate(out PlateKitchenObject plateKitchenObject))
            {
                if (plateKitchenObject.TryAddingIngredient(GetKitchenObject().GetKitchenObjectSO()))
                    KitchenObject.DestroyKitchenObject(GetKitchenObject());
            }
            else if (GetKitchenObject().TryGettingPlate(out plateKitchenObject))
                if (plateKitchenObject.TryAddingIngredient(player.GetKitchenObject().GetKitchenObjectSO()))
                    KitchenObject.DestroyKitchenObject(player.GetKitchenObject());
        }
        else GetKitchenObject().SetKitchenObjectParent(player);
    }
}
