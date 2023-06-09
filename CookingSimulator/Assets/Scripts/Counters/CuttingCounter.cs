using System;
using UnityEngine;
using Unity.Netcode;

public class CuttingCounter : BaseCounter, IHasProgress
{
    [SerializeField] private CuttingRecipeSO[] cuttingRecipeObjectSOArray;
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    public event EventHandler OnCut;
    public static event EventHandler OnAnyCut;

    private int cuttingProgress;

    public override void Interact(Player player)
    {
        if (!HasKitchenObject())
        {
            if (player.HasKitchenObject())
                if (HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO()))
                {
                    KitchenObject kitchenObject = player.GetKitchenObject();
                    kitchenObject.SetKitchenObjectParent(this);
                    InteractLogicPlateObjectOnCounterServerRpc();
                }
        }
        else if (player.HasKitchenObject())
        {
            if (player.GetKitchenObject().TryGettingPlate(out PlateKitchenObject plateKitchenObject))
                if (plateKitchenObject.TryAddingIngredient(GetKitchenObject().GetKitchenObjectSO()))
                    KitchenObject.DestroyKitchenObject(GetKitchenObject());
        }
        else GetKitchenObject().SetKitchenObjectParent(player);
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicPlateObjectOnCounterServerRpc()
    {
        InteractLogicPlateObjectOnCounterClientRpc();
    }

    [ClientRpc]
    private void InteractLogicPlateObjectOnCounterClientRpc()
    {
        cuttingProgress = 0;

        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs { progressNormnalized = 0f });
    }

    public override void InteractAlternate(Player player)
    {
        if (HasKitchenObject() && HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSO()))
        {
            CutObjectServerRpc();
            TestCuttingProgressDoneServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CutObjectServerRpc()
    {
        CutObjectClientRpc();
    }

    [ClientRpc]
    private void CutObjectClientRpc()
    {
        cuttingProgress++;
        OnCut?.Invoke(this, EventArgs.Empty);
        OnAnyCut?.Invoke(this, EventArgs.Empty);

        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        { progressNormnalized = (float)cuttingProgress / cuttingRecipeSO.cuttingProgressMax });
    }

    [ServerRpc(RequireOwnership = false)]
    private void TestCuttingProgressDoneServerRpc()
    {
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());

        if (cuttingProgress >= cuttingRecipeSO.cuttingProgressMax)
        {
            KitchenObjectSO outputKitchenObjectSO = GetOutputForInput(GetKitchenObject().GetKitchenObjectSO());

            KitchenObject.DestroyKitchenObject(GetKitchenObject());
            KitchenObject.SpawnKitchenObject(outputKitchenObjectSO, this);
        }
    }

    private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(inputKitchenObjectSO);
        return cuttingRecipeSO != null;
    }

    private KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSO)
    {
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(inputKitchenObjectSO);
        if (cuttingRecipeSO) return cuttingRecipeSO.output;

        return null;
    }

    private CuttingRecipeSO GetCuttingRecipeSOWithInput (KitchenObjectSO inputKitchenObjectSO)
    {
        foreach (CuttingRecipeSO cuttingRecipeSO in cuttingRecipeObjectSOArray)
            if (cuttingRecipeSO.input == inputKitchenObjectSO) return cuttingRecipeSO;

        return null;
    }

    new public static void ResetStaticData()
    {
        OnAnyCut = null;
    }
}
