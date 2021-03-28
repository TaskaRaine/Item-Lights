using HarmonyLib;
using System;
using Vintagestory.API.Common;

namespace ItemLights
{
    //-- Changes the light of the player based on the item lights that may be in their hand --//
    [HarmonyPatch(typeof(EntityPlayer), "LightHsv", MethodType.Getter)]
    public class ItemHandLight
    {
        static byte[] Postfix(byte[] __result, EntityPlayer __instance)
        {
            try
            {
                byte[] rightItemLight = new byte[] { };
                byte[] leftItemLight = new byte[] { };
                byte[] finalItemLight;

                if (!__instance.RightHandItemSlot.Empty)
                    if (__instance.RightHandItemSlot.Itemstack.Collectible.Attributes != null)
                        if (__instance.RightHandItemSlot.Itemstack.Collectible.Attributes.KeyExists("itemlight"))
                            rightItemLight = __instance.RightHandItemSlot.Itemstack.Collectible.Attributes["itemlight"].AsArray<byte>();

                if (!__instance.LeftHandItemSlot.Empty)
                    if (__instance.LeftHandItemSlot.Itemstack.Collectible.Attributes != null)
                        if (__instance.LeftHandItemSlot.Itemstack.Collectible.Attributes.KeyExists("itemlight"))
                            leftItemLight = __instance.LeftHandItemSlot.Itemstack.Collectible.Attributes["itemlight"].AsArray<byte>();

                //-- If both hands are holding an item light, blend colours between them --//
                if (rightItemLight.Length == 3 && leftItemLight.Length == 3)
                {
                    finalItemLight = new byte[] {
                        (byte)((rightItemLight[0] + leftItemLight[0]) / 2),
                        (byte)((rightItemLight[1] + leftItemLight[1]) / 2),
                        (byte)Math.Max(rightItemLight[2], leftItemLight[2])
                    };
                }
                else if (rightItemLight.Length == 3)
                {
                    //-- If __result is set then it means a non-itemlight item has already set the player light. --//
                    //-- Blend the previous light value with the right item light value --//
                    if (__result != null && __result.Length == 3)
                    {
                        finalItemLight = new byte[]
                        {
                            (byte)((rightItemLight[0] + __result[0]) / 2),
                            (byte)((rightItemLight[1] + __result[1]) / 2),
                            (byte)Math.Max(rightItemLight[2], __result[2])
                        };
                    }
                    else
                        finalItemLight = rightItemLight;
                }
                else if (leftItemLight.Length == 3)
                {
                    //-- Blend the previous light value with the left item light value --//
                    if (__result != null && __result.Length == 3)
                    {
                        finalItemLight = new byte[]
                        {
                            (byte)((leftItemLight[0] + __result[0]) / 2),
                            (byte)((leftItemLight[1] + __result[1]) / 2),
                            (byte)Math.Max(leftItemLight[2], __result[2])
                        };
                    }
                    else
                        finalItemLight = leftItemLight;
                }
                else
                {
                    //-- If everything else fails, just set the light to __result received from the vanilla method. --//
                    finalItemLight = __result;
                }

                return finalItemLight;
            }
            catch
            {
                return __result;
            }
        }

        //-- Sets the entity light of the dropped item to be the light attribute if it exists --//
        [HarmonyPatch(typeof(EntityItem), "LightHsv", MethodType.Getter)]
        public class ItemEntityLight
        {
            static byte[] Postfix(byte[] __result, EntityItem __instance)
            {
                try
                {
                    byte[] finalLight = __result;

                    if(__instance.Itemstack.Collectible.Attributes != null)
                        if (__instance.Itemstack.Collectible.Attributes.KeyExists("itemlight"))
                            finalLight = __instance.Itemstack.Collectible.Attributes["itemlight"].AsArray<byte>();

                    return finalLight;
                }
                catch
                {
                    return __result;
                }
            }
        }
    }
}
