﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CGO
{
    /// <summary>
    /// A parameter used for instantiation of a component in an entity from a template.
    /// </summary>
    public class ComponentParameter
    {
        private string m_memberName;
     
        /// <summary>
        /// The name of the component member
        /// </summary>
        public string MemberName
        {
            get
            { return m_memberName; }
            set { }
        }

        private string m_parameterType;
        /// <summary>
        /// The type of parameter specified
        /// </summary>
        public string ParameterType
        {
            get
            { return m_parameterType; }
            set { }
        }

        private object m_parameter;
        /// <summary>
        /// The parameter object
        /// </summary>
        public object Parameter
        {
            get
            { return m_parameter; }
            set { }
        }

        public ComponentParameter(string memberName, string parameterType, object parameter)
        {
            m_memberName = memberName;
            m_parameterType = parameterType;
            m_parameter = parameter;
        }
    }
}
