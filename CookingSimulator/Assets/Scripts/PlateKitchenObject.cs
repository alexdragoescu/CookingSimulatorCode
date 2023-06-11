using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlateKitchenObject : KitchenObject
{
    public event EventHandler<OnIngredientAddedEventArgs> OnIngredientAdded;

    public class OnIngredientAddedEventArgs : EventArgs { public KitchenObjectSO kitchenObjectSO; }

    [SerializeField] private List<KitchenObjectSO> validKitchenObjectSOList;
    private List<KitchenObjectSO> kitchenObjectSOList;

    private int cookedMeatIndex = 4;
    private int burnedMeatIndex = 5;
    private int uncookedMeatIndex = 6;

    protected override void Awake()
    {
        base.Awake();
        kitchenObjectSOList = new List<KitchenObjectSO>();
    }

    public bool TryAddingIngredient(KitchenObjectSO kitchenObjectSO)
    {
        if (!validKitchenObjectSOList.Contains(kitchenObjectSO)) return false;
        if (kitchenObjectSOList.Contains(kitchenObjectSO)) return false;
        if (!CheckMeat(kitchenObjectSO)) return false;

        AddIngredientServerRpc(KitchenGameMultiplayer.Instance.GetKitchenObjectSOIndex(kitchenObjectSO));
        return true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddIngredientServerRpc(int kitchenObjectSOIndex)
    {
        AddIngredientClientRpc(kitchenObjectSOIndex);
    }

    [ClientRpc]
    private void AddIngredientClientRpc(int kitchenObjectSOIndex)
    {
        KitchenObjectSO kitchenObjectSO = KitchenGameMultiplayer.Instance.GetKitchenObjectSOFromIndex(kitchenObjectSOIndex);
        kitchenObjectSOList.Add(kitchenObjectSO);
        OnIngredientAdded?.Invoke(this, new OnIngredientAddedEventArgs { kitchenObjectSO = kitchenObjectSO });
    }

    public List<KitchenObjectSO> GetKitchenObjectSOList()
    {
        return kitchenObjectSOList;
    }

    private bool CheckMeat(KitchenObjectSO kitchenObjectSO)
    {
        if (kitchenObjectSO == validKitchenObjectSOList[cookedMeatIndex]
            && (kitchenObjectSOList.Contains(validKitchenObjectSOList[burnedMeatIndex])) || (kitchenObjectSOList.Contains(validKitchenObjectSOList[uncookedMeatIndex])))
            return false;

        if (kitchenObjectSO == validKitchenObjectSOList[burnedMeatIndex]
            && (kitchenObjectSOList.Contains(validKitchenObjectSOList[cookedMeatIndex])) || (kitchenObjectSOList.Contains(validKitchenObjectSOList[uncookedMeatIndex])))
            return false;

        if (kitchenObjectSO == validKitchenObjectSOList[uncookedMeatIndex]
            && (kitchenObjectSOList.Contains(validKitchenObjectSOList[cookedMeatIndex])) || (kitchenObjectSOList.Contains(validKitchenObjectSOList[burnedMeatIndex])))
            return false;

        return true;
    }
}
