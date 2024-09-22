using Anvil.API;
using HackyJunk;
using NLog;
using NWN.Native.API;
using YamlDotNet.Core.Tokens;
using ItemProperty = Anvil.API.ItemProperty;

namespace SamuelIH.Nwn.Blueprints.Anvil;

public static class ItemCreator
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    
    public static CNWSItem CreateItem(ItemBlueprint blueprint)
    {
        var item = new CNWSItem(2130706432);
        GC.SuppressFinalize(item);
        
        item.m_nArmorValue = blueprint.ArmorValue ?? 0;
        item.m_nBaseItem = blueprint.BaseItem!.Value;
        
        item.InitRepository(item.m_idSelf);
        item.m_nNumCharges = blueprint.Charges ?? 0;
        item.m_bIdentified = (blueprint.Identified ?? true) ? 1 : 0;
        item.m_sIdentifiedDescription = (blueprint.LocalizedDescription ?? "").ToExoLocString();
        if (blueprint.LocalizedName is string name) item.m_sName = name.ToExoLocString();
        
        if (blueprint.ModelParts is byte[] parts)
        {
            item.m_nModelPart[0] = parts[0];
            item.m_nModelPart[1] = parts[1];
            item.m_nModelPart[2] = parts[2];
        }
        
        item.m_bPlotObject = blueprint.Plot ?? false ? 1 : 0;

        var nwItem = item.ToNwObject<NwItem>()!;
        
        if (blueprint.Properties is ItemBlueprint.Property[] properties)
            foreach (var t in properties)
            {
                WriteProperty(t, item, nwItem);
            }
        
        if (blueprint.PropertiesList is OverridableList<ItemBlueprint.Property> propertiesList)
            foreach (var t in propertiesList)
            {
                WriteProperty(t, item, nwItem);
            }
        
        item.m_sTag = (blueprint.Tag ?? "").ToExoString();
        
        item.m_nLayeredTextureColors[0] = blueprint.Cloth1Color ?? 0;
        item.m_nLayeredTextureColors[1] = blueprint.Cloth2Color ?? 0;
        item.m_nLayeredTextureColors[2] = blueprint.Leather1Color ?? 0;
        item.m_nLayeredTextureColors[3] = blueprint.Leather2Color ?? 0;
        item.m_nLayeredTextureColors[4] = blueprint.Metal1Color ?? 0;
        item.m_nLayeredTextureColors[5] = blueprint.Metal2Color ?? 0;
        
        
        item.m_nArmorModelPart[0] = blueprint.ArmorPartRFoot ?? 0;
        item.m_nArmorModelPart[1] = blueprint.ArmorPartLFoot ?? 0;
        item.m_nArmorModelPart[2] = blueprint.ArmorPartRShin ?? 0;
        item.m_nArmorModelPart[3] = blueprint.ArmorPartLShin ?? 0;
        item.m_nArmorModelPart[4] = blueprint.ArmorPartLThigh ?? 0;
        item.m_nArmorModelPart[5] = blueprint.ArmorPartRThigh ?? 0;
        item.m_nArmorModelPart[6] = blueprint.ArmorPartPelvis ?? 0;
        item.m_nArmorModelPart[7] = blueprint.ArmorPartTorso ?? 0;
        item.m_nArmorModelPart[8] = blueprint.ArmorPartBelt ?? 0;
        item.m_nArmorModelPart[9] = blueprint.ArmorPartNeck ?? 0;
        item.m_nArmorModelPart[10] = blueprint.ArmorPartRForearm ?? 0;
        item.m_nArmorModelPart[11] = blueprint.ArmorPartLForearm ?? 0;
        item.m_nArmorModelPart[12] = blueprint.ArmorPartRBicep ?? 0;
        item.m_nArmorModelPart[13] = blueprint.ArmorPartLBicep ?? 0;
        item.m_nArmorModelPart[14] = blueprint.ArmorPartRShoulder ?? 0;
        item.m_nArmorModelPart[15] = blueprint.ArmorPartLShoulder ?? 0;
        item.m_nArmorModelPart[16] = blueprint.ArmorPartRHand ?? 0;
        item.m_nArmorModelPart[17] = blueprint.ArmorPartLHand ?? 0;
        item.m_nArmorModelPart[18] = blueprint.ArmorPartRobe ?? 0;

        if (blueprint.IntVars is not null)
        {
            foreach (var (key, value) in blueprint.IntVars)
            {
                item.m_ScriptVars.SetInt(key.ToExoString(), value);
            }
        }
        
        if (blueprint.StrVars is not null)
        {
            foreach (var (key, value) in blueprint.StrVars)
            {
                item.m_ScriptVars.SetString(key.ToExoString(), value.ToExoString());
            }
        }
        
        // compute things
        item.ComputeWeight();
        
        return item;
    }

    private static void WriteProperty(ItemBlueprint.Property property, CNWSItem toNativeItem, NwItem toItem)
    {
        ItemProperty iProp;
        switch (property.PropertyName)
        {
            case 35:
                iProp = ItemProperty.Haste();
                break;
            case 43:
                iProp = ItemProperty.Keen();
                break;
            case 83:
                iProp = ItemProperty.VisualEffect((ItemVisual)property.SubType);
                break;
            default:
                iProp = ItemProperty.Custom(property.PropertyName, property.SubType, property.CostTableValue, property.Param1Value ?? -1);
                break;
        }

        toItem.AddItemProperty(iProp, EffectDuration.Permanent);
    }
}