using System;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.Threading.Tasks;

using ConsoleApplication3.Interfaces;
using ConsoleApplication3.Implementations;

namespace ConsoleApplication3
{
    public static class ElementFactory
    {
        private static readonly Dictionary<string, Type> wellKnownElements = new Dictionary<string, Type>() { 
         { "switch_app", typeof(ConsoleApplication3.Elements.switchElement)} //Переключить контекст
        ,{ "mt_set_var", typeof(ConsoleApplication3.Elements.mtSetVarElement)} //Установать переменную
        ,{ "mt_test_exec", typeof(ConsoleApplication3.Elements.mtTestExecElement)} //Выполнить тест
        ,{ "mt_save_result", typeof(ConsoleApplication3.Elements.mtSaveResultElement)} //Сохранить результат
        };

        public static IElement makeElement(IBlock block, Dictionary<string,string> params_set, XmlNode root)
        {
            IConfig conf = new Config();
            conf.updateFrom(params_set);

            string Name = conf["Name"];
            string Id = conf["Id"];
                                            
            if (String.IsNullOrEmpty(Name)) throw new ArgumentNullException("Не задано имя элемента");
            if (!wellKnownElements.ContainsKey(Name)) throw new NotImplementedException(String.Format("Неизвестное имя элемента \"{0}\"",Name));

            

            Dictionary<string, string> dict = new Dictionary<string, string>();

            dict.Add("SessionId", block.test.testRun.Id);

            foreach (XmlNode child in root.ChildNodes)
            {
                XmlAttributeCollection pars = child.Attributes;
                XmlAttribute name = pars["name"];
                XmlAttribute value = pars["value"];
                dict.Add(name.Value, value.Value);
            }

            Type type = wellKnownElements[Name];

            object Element = Activator.CreateInstance(type);
            IElement elem = Element as IElement;
            elem.Name = Name;
            elem.Id = Id;
            elem.Block = block;
            elem.Initialize(dict);
            return elem;
        }
    }
}
