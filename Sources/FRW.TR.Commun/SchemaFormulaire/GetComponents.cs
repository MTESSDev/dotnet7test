using FRW.TR.Commun.Utils;
using FRW.TR.Contrats.Composants;
using System;
using System.Collections.Generic;

namespace FRW.TR.Commun.SchemaFormulaire
{
    public static class Component
    {
        /// <summary>
        /// Permet de copier les infos d'une config par défaut vers la config d'un formulaire.
        /// </summary>
        /// <param name="form"></param>
        /// <param name="defaults"></param>
        public static void MergeProp(dynamic form, dynamic defaults)
        {
            if (defaults is null) return;

            var componentsList = form as IList<object>;

            if (componentsList != null)
            {
                foreach (var component in componentsList)
                {
                    var componentDict = component as IDictionary<object, object>;

                    if (componentDict != null)
                    {
                        if (componentDict.TryGetValue("sections", out var sections))
                        {
                            MergeProp(sections, defaults);
                        }

                        if (componentDict.TryGetValue("components", out var components))
                        {
                            MergeProp(components, defaults);
                        }

                        var defDict = defaults as IDictionary<object, object>;
                        if (componentDict.TryGetValue("type", out var currentType))
                            if (defDict!.TryGetValue(currentType, out var defaultsVal))
                            {
                                foreach (var attr in (defaultsVal as IDictionary<object, object>)!)
                                {
                                    if (!componentDict.TryAdd(attr.Key, attr.Value))
                                    {
                                        if (componentDict[attr.Key] is IDictionary<object, object> compDict)
                                            compDict.TryAdd(attr.Value as IDictionary<object, object>);
                                        else
                                           if (attr.Key.Equals("type"))
                                            componentDict["type"] = attr.Value;
                                    }
                                }
                            }
                    }
                }
            }
        }

        public static void GetComponents(bool listerValeurs, object components, ref List<ComponentBinding> formData, string? sectionId = null, object? sectionName = null, string? prefixId = null, string? groupName = null, object? groupNameIntl = null, object? groupInstanceNameIntl = null, bool? isRepeatable = null, int? maxOccur = null, object? sectionGroupName = null, object? parentType = null)
        {
            var componentsList = components as IList<object>;

            if (componentsList != null)
            {
                foreach (var component in componentsList)
                {
                    var componentDict = component as Dictionary<object, object>;

                    if (componentDict != null)
                    {
                        if (componentDict.TryGetValue("sectionGroup", out var sectionGroup))
                        {
                            sectionGroupName = sectionGroup;
                        }

                        if (componentDict.TryGetValue("sections", out var sections))
                        {
                            componentDict.TryGetValue("prefixId", out var currentPrefixId);

                            GetComponents(listerValeurs, sections, ref formData, sectionId, sectionName, currentPrefixId?.ToString(), null, null, null, null, null, sectionGroupName);
                        }

                        if (componentDict.TryGetValue("components", out var innerComponents))
                        {
                            componentDict.TryGetValue("id", out var sectionIdTemp);
                            if (sectionIdTemp != null)
                            {
                                sectionId = sectionIdTemp.ToString();
                            }

                            componentDict.TryGetValue("section", out var sectionNameTemp);
                            if (sectionNameTemp != null)
                            {
                                sectionName = sectionNameTemp;
                            }

                            string? newGroupName = groupName;
                            object? newGroupNameIntl = groupNameIntl;
                            object? newGroupInstanceNameIntl = groupInstanceNameIntl;
                            int? newMaxOccur = maxOccur;
                            bool? newIsRepeatable = isRepeatable;

                            if (componentDict.TryGetValue("type", out var componentType))
                            {
                                if (componentType != null)
                                {
                                    if (componentType.ToString()!.EndsWith("group", StringComparison.InvariantCultureIgnoreCase) || componentType.ToString()!.EndsWith("adresse", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        newGroupName = componentDict["name"].ToString();

                                        newGroupNameIntl = componentDict.GetValueOrDefault("label");
                                        newGroupInstanceNameIntl = componentDict.GetValueOrDefault("instanceLabel");

                                        if (componentDict.ContainsKey("limit"))
                                        {
                                            newMaxOccur = int.Parse(componentDict["limit"].ToString() ?? "0");
                                        }

                                        if (componentDict.TryGetValue("repeatable", out var estRepetable))
                                        {
                                            newIsRepeatable = bool.Parse(estRepetable.ToString() ?? "false");
                                        }
                                    }
                                }
                            }

                            GetComponents(listerValeurs, innerComponents, ref formData, sectionId, sectionName, prefixId, newGroupName, newGroupNameIntl, newGroupInstanceNameIntl, newIsRepeatable, newMaxOccur, sectionGroupName, componentType);
                        }
                    }

                    var inputV = new ComponentBinding();
                    inputV.ParseAttributes(componentDict);
                    inputV.SectionGroupNameIntl = sectionGroupName;
                    inputV.GroupName = groupName;
                    inputV.ParentType = parentType?.ToString();
                    inputV.GroupNameIntl = groupNameIntl;
                    inputV.GroupInstanceNameIntl = groupInstanceNameIntl;
                    inputV.SectionName = sectionId;
                    inputV.SectionNameIntl = sectionName;
                    inputV.PrefixId = prefixId;
                    inputV.IsRepeatable = isRepeatable ?? false;
                    inputV.MaxOccur = maxOccur;
                    inputV.NameValues = new List<string>();

                    if (inputV.Type != TypeInput.SKIP || (!listerValeurs && inputV.HasVIf))
                    {
                        inputV.NameValues.AddRange(GenerateElementName(inputV, null));

                        if (listerValeurs && inputV.AcceptedValues != null)
                        {
                            foreach (var val in inputV.AcceptedValues)
                            {
                                inputV.NameValues.AddRange(GenerateElementName(inputV, $"=={val.Key}"));
                            }
                        }

                        formData.Add(inputV);
                    }
                }
            }
        }

        public static IEnumerable<string> GenerateElementName(ComponentBinding input, string? value, int index = 0)
        {
            if (input.IsGroup)
            {
                if (input.IsRepeatable!)
                {
                    for (int i = index; i < input.MaxOccur; i++)
                    {
                        yield return $"{input.FullGroupName}.{i}.{input.FullName}{value}";
                    }
                }
                else
                {
                    yield return $"{input.FullGroupName}.0.{input.FullName}{value}";
                }
            }
            else
            {
                yield return $"{input.FullName}{value}";
            }
        }
    }

}