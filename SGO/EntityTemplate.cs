﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SGO
{
    /// <summary>
    /// This class holds a template for an entity -- the entity name, components, and parameters the entity will be instantiated with.
    /// </summary>
    public class EntityTemplate
    {
        /// <summary>
        /// This holds a list of the component types the entity will be instantiated with.
        /// </summary>
        private List<string> components = new List<string>();
        /// <summary>
        /// This holds a dictionary linking parameter objects to 
        /// </summary>
        private Dictionary<string, List<ComponentParameter>> parameters = new Dictionary<string, List<ComponentParameter>>();

        /// <summary>
        /// The Placement mode used for server-initiated placement. This is used for placement during normal gameplay. The clientside version controls the placement type for editor and admin spawning.
        /// </summary>
        public PlacementOption placementMode { get; private set; }

        /// <summary>
        /// The Range this entity can be placed from.
        /// </summary>
        public int placementRange { get; private set; }

        /// <summary>
        /// Name of the entity template eg. "HumanMob"
        /// </summary>
        private string m_name;
        public string Name
        {
            get
            { return m_name; }
            set
            { m_name = value; }
        }
        
        /// <summary>
        /// Default constructor
        /// </summary>
        public EntityTemplate()
        {

        }

        /// <summary>
        /// Creates an entity from this template
        /// </summary>
        /// <returns></returns>
        public Entity CreateEntity()
        {
            Entity e = new Entity();

            foreach (string componentname in components)
            {
                IGameObjectComponent component = ComponentFactory.Singleton.GetComponent(componentname);
                if (component == null)
                    continue; //TODO THROW ERROR

                ///Get all the params in the template that apply to this component
                var cparameters = parameters[componentname];
                foreach (ComponentParameter p in cparameters)
                {
                    ///Set the component's parameters
                    component.SetParameter(p);
                }
                ///Add the component to the entity
                e.AddComponent(component.Family, component);
            }
            e.name = Name;
            e.template = this;
            return e;
        }

        /// <summary>
        /// Adds a component type to the entity template
        /// </summary>
        public void AddComponent(string componentType)
        {
            components.Add(componentType);
        }

        /// <summary>
        /// Sets a parameter for a component type for this template
        /// </summary>
        /// <param name="t">The type of the component to set a parameter on</param>
        /// <param name="parameter">The parameter object</param>
        public void SetParameter(string componenttype, ComponentParameter parameter)
        {
            if (parameters.ContainsKey(componenttype))
                parameters[componenttype].Add(parameter);
        }

        public void LoadFromXML(XElement templateElement)
        {
            Name = templateElement.Attribute("name").Value;

            var t_components = templateElement.Element("Components").Elements();
            //Parse components
            foreach (XElement t_component in t_components)
            {
                string componentname = t_component.Attribute("name").Value;
                components.Add(componentname);
                parameters.Add(componentname, new List<ComponentParameter>());
                var t_componentParameters = from t_param in t_component.Descendants("Parameter")
                                            select t_param;
                //Parse component parameters
                foreach (XElement t_componentParameter in t_componentParameters)
                {
                    Type paramtype = translateType(t_componentParameter.Attribute("type").Value);
                    if (paramtype == null)
                        break; //TODO THROW ERROR
                    parameters[componentname].Add(new ComponentParameter(t_componentParameter.Attribute("name").Value,
                                                                         paramtype,
                                                                         t_componentParameter.Attribute("value").Value)
                                                 );
                }

                if (t_component.Element("ExtendedParameters") != null)
                {
                    parameters[componentname].Add(new ComponentParameter("ExtendedParameters", typeof(XElement), t_component.Element("ExtendedParameters")));
                }
            }

            var t_placementprops = templateElement.Element("PlacementProperties");
            //Load Placement properties.
            if (t_placementprops != null)
            {
                XElement modeElement = t_placementprops.Element("PlacementMode");
                XElement rangeElement = t_placementprops.Element("PlacementRange");

                if (modeElement != null)
                {
                    string modeName = modeElement.Attribute("type").Value;
                    this.placementMode = (PlacementOption)Enum.Parse(typeof(PlacementOption), modeName);
                }
                else
                    this.placementMode = PlacementOption.AlignNone;

                if (rangeElement != null)
                {
                    int range = int.Parse(rangeElement.Attribute("value").Value);
                    this.placementRange = range;
                }
                else
                    this.placementRange = 200;
            }
        }

        private Type translateType(string typeName)
        {
            switch (typeName.ToLowerInvariant())
            {
                case "string":
                    return typeof(string);
                case "int":
                    return typeof(int);
                case "float":
                    return typeof(float);
                default:
                    return null;
            }
        }
    }
}
