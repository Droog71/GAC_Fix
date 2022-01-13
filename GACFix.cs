using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GACFix : FortressCraftMod
{
    // Mod registry
    public override ModRegistrationData Register()
    {
        ModRegistrationData modRegistrationData = new ModRegistrationData();
        return modRegistrationData;
    }

    public IEnumerator Start()
    {
        foreach (ModConfiguration mod in ModManager.mModConfigurations.Mods)
        {
            string path = Path.Combine(mod.Path, "Xml/GenericAutoCrafter");
            if (Directory.Exists(path))
            {
                string[] array = Directory.GetFiles(path, "*.xml");
                foreach (string obj in array)
                {
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(obj);
                    TerrainData.mXMLReadInProgress = obj;
                    GenericAutoCrafterDataEntry genericAutoCrafterDataEntry = (GenericAutoCrafterDataEntry)XMLParser.ReadXML(obj, typeof(GenericAutoCrafterDataEntry));
                    if (genericAutoCrafterDataEntry.Custom != null)
                    {
                        genericAutoCrafterDataEntry.Custom.Distribute();
                    }
                    for (int j = 0; j < GenericAutoCrafterNew.mMachines.Count; j++)
                    {
                        if (GenericAutoCrafterNew.mMachines[j].Value == genericAutoCrafterDataEntry.Value)
                        {
                            GenericAutoCrafterNew.mMachines.Remove(GenericAutoCrafterNew.mMachines[j]);
                        }
                    }
                }
                foreach (string text in array)
                {
                    try
                    {
                        string fileNameWithoutExtension2 = Path.GetFileNameWithoutExtension(text);
                        OverrideGenericAutoCrafterDataEntry overrideGenericAutoCrafterDataEntry = (OverrideGenericAutoCrafterDataEntry)XMLParser.ReadXML(text, typeof(OverrideGenericAutoCrafterDataEntry));
                        if (overrideGenericAutoCrafterDataEntry != null)
                        {
                            if (overrideGenericAutoCrafterDataEntry.Custom != null)
                            {
                                overrideGenericAutoCrafterDataEntry.Custom.Distribute();
                            }
                            if (overrideGenericAutoCrafterDataEntry.IsOverride)
                            {
                                if (GenericAutoCrafterNew.mMachinesByKey.TryGetValue(fileNameWithoutExtension2, out GenericAutoCrafterDataEntry value))
                                {
                                    value.ApplyOverrides(overrideGenericAutoCrafterDataEntry);
                                }
                                else
                                {
                                    Debug.LogWarning("[Modding] Mod '" + mod.Id + "' version " + mod.Version + " is attempting to override generic auto crafting machine '" + fileNameWithoutExtension2 + "' however this machine was not found. Override has been ignored, check mod dependencies.");
                                }
                            }
                            else if (!fileNameWithoutExtension2.Contains("."))
                            {
                                Debug.LogError("[Modding] Mod " + mod.Id + " version " + mod.Version + " is attempting to add a generic auto crafter with an invalid key (" + fileNameWithoutExtension2 + "). Keys must start with your unique Author id, followed by a period (.), followed by the rest of the id.");
                            }
                            else if (fileNameWithoutExtension2.StartsWith("Example.", StringComparison.InvariantCultureIgnoreCase))
                            {
                                Debug.LogError("[Modding]  Mod " + mod.Id + " version " + mod.Version + " is attempting to add a generic auto crafter with an invalid key. You need to pick a unique author id. 'Example' is banned to prevent mods being released with conflicting ids as a result of copy-paste from the guide.");
                            }
                            else if (GenericAutoCrafterNew.mMachinesByKey.ContainsKey(fileNameWithoutExtension2))
                            {
                                Debug.LogWarning("[Modding] Mod '" + mod.Id + "' version " + mod.Version + " is attempting to add machine '" + fileNameWithoutExtension2 + "' however a machine with this key already exists, machine has been ignored. If this mod was intending to override the existing machine then the IsOverride tag is required.");
                            }
                            else
                            { 
                                GenericAutoCrafterDataEntry genericAutoCrafterDataEntry2 = overrideGenericAutoCrafterDataEntry.ToStandardFormat();
                                GenericAutoCrafterNew.mMachines.Add(genericAutoCrafterDataEntry2);
                                GenericAutoCrafterNew.mMachinesByKey.Add(fileNameWithoutExtension2, genericAutoCrafterDataEntry2);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex.InnerException == null)
                        {
                            Debug.LogError("An error occurred loading generic auto crafting machine mod file '" + text + "'. [" + ex.Message + "][NO_INNER_EXCEPTION]");
                        }
                        else
                        {
                            Debug.LogError("An error occurred loading generic auto crafting machine mod file '" + text + "'. [" + ex.Message + "][" + ex.InnerException.Message + "]");
                        }
                    }
                }
            }
        }
        foreach (GenericAutoCrafterDataEntry mMachine2 in GenericAutoCrafterNew.mMachines)
        {
            if (!TerrainData.mEntryValuesByKey.TryGetValue(mMachine2.Value, out TerrainDataValueEntry value2))
            {
                Debug.LogError("Unable to find terrain value entry for generic auto crafter with key '" + mMachine2.Value + "'. Please ensure you are using the key, not the number.");
            }
            else
            {
                mMachine2.CubeValue = value2.Value;
                if (!GenericAutoCrafterNew.mMachinesByValue.ContainsKey(mMachine2.CubeValue))
                {
                    GenericAutoCrafterNew.mMachinesByValue.Add(mMachine2.CubeValue, mMachine2);
                }
                if (mMachine2.Recipe != null)
                {
                    CraftData.LinkEntries(new List<CraftData>
                    {
                        mMachine2.Recipe
                    }, "GenericAutoCrafter");
                }
            }
        }
        yield return null;
    }
}