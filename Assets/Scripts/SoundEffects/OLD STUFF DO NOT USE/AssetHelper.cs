using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class AssetHelper {
    static UnityEngine.Object GetAsset(AssetReference assetReference) {
        if (assetReference.Asset != null)
            return assetReference.Asset;

        UnityEngine.Object o = null;

        assetReference.LoadAssetAsync<UnityEngine.Object>().Completed +=
                    (asyncOperationHandle) => {
                        if (asyncOperationHandle.Status == AsyncOperationStatus.Succeeded) {
                            o = asyncOperationHandle.Result;
                        }
                    };
        return o;
    }
}
