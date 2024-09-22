using Anvil.API;
using Anvil.Services;
using HackyJunk;
using NLog;
using NWN.Native.API;

namespace SamuelIH.Nwn.Blueprints.Anvil;

[ServiceBinding(typeof(Instantiator))]
public class Instantiator
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    private static string InstantiatorResrefVarName = "Instantiator_Resref";

    public Loader Loader { get; private set; } = new Loader
        {Log = new LoggingBridge
        {
            Error = Log.Error,
            Warning = Log.Warn,
            Info = Log.Info,
        }};
    
    public NwItem? CreateItem(string blueprintName, bool allowVanillaFallback = true)
    {
        if (Loader.GetBlueprint(blueprintName) is not ItemBlueprint blueprint)
        {
            if (allowVanillaFallback)
            {
                if (NwItem.Create(blueprintName, NwModule.Instance.StartingLocation) is not NwItem vanillaItem)
                    return null;
                var nativeVanillaItem = (CNWSItem)vanillaItem!;
                nativeVanillaItem.RemoveFromArea();
                return vanillaItem;
            }

            Log.Error($"Blueprint {blueprintName} not found.");
            return null;
        }


        var item = ItemCreator.CreateItem(blueprint);
        
        if (item.ToNwObject<NwItem>() is not NwItem managedItem) return null;
        managedItem.GetObjectVariable<LocalVariableString>(InstantiatorResrefVarName).Value = blueprintName;
        return managedItem;
    }

    /// <summary>
    /// Creates the specified item on the game object
    /// </summary>
    /// <param name="blueprintName">item to create</param>
    /// <param name="gameObject">creature to creat it on</param>
    /// <param name="amount">Amount to create. If not specified, it will default to the stacksize on the blueprint.
    /// If specified, it will overwrite the stack size and potentially create multiples to satisfy the constraint.</param>
    /// <param name="allowVanillaFallback">If true, and there is no blueprint with the matching resref, it may create one using the vanilla blueprints.</param>
    public NwItem? CreateItemOn(string blueprintName, NwGameObject gameObject, int? amount = null, bool allowVanillaFallback = true)
    {
        if (CreateItem(blueprintName, allowVanillaFallback) is not NwItem item) return null;
        
        NwItem? result = null;
        if (amount is int targetAmount)
        {
            var maxStackSize = item.BaseItem.MaxStackSize;
            while (targetAmount > 0)
            {
                var stackSize = Math.Min(targetAmount, maxStackSize);
                targetAmount -= stackSize;
            
                item.StackSize = stackSize;
                result = item.Clone(gameObject);
            }
        }
        else
        {
            result = item.Clone(gameObject);
        }
        
        item.Destroy();
        return result;
    }

    public string? GetBlueprintName(NwItem item, bool allowTagFallback = true)
    {
        return item.GetObjectVariable<LocalVariableString>(InstantiatorResrefVarName).Value ??
               (allowTagFallback ? item.Tag.ToLower() : null);
    }
    
    public ItemBlueprint? GetBlueprint(NwItem item)
    {
        if (GetBlueprintName(item, false) is not string iResRef)
            return null;
        return Loader.GetBlueprint(iResRef);
    }
}
