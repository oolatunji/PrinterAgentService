using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Xml;
using System.Configuration;

namespace PASLibrary
{
    public class DictionaryFactory
    {
        /// <summary>
        /// Chargement d'une section d'un fichier xml
        /// </summary>
        /// <param name="fileName">le nom du fichier</param>
        /// <param name="sectionName">la section à charger</param>
        /// <returns>null en cas d'echec et un IDictionary contenant la section sinon</returns>
        public static IDictionary Load(string fileName, string sectionName)
        {
            if (!System.IO.File.Exists(fileName))
            {
                return null;
            }

            XmlTextReader reader;
            try
            {
                reader = new XmlTextReader(fileName);
            }
            catch (Exception)
            {
                return null;
            }

            XmlDocument dicoFile = new XmlDocument();
            try
            {
                dicoFile.Load(reader);
            }
            catch (Exception)
            {

                dicoFile = null;  // on délégue la gestion de l'erreur à GetConfig
            }

            reader.Close();
            return GetConfig(sectionName, dicoFile);
        }

        /// <summary>
        /// Sauve dans un fichier et une section donnée, un dictionnaire (string, string)
        /// </summary>
        /// <param name="dico">le dictionnaire à sauver</param>
        /// <param name="filename">le nom du fichier XML</param>
        /// <param name="sectionName">le nom de la section spécifique du XML</param>
        /// <exception cref="KeyServerException"/>
        public static void Save(IDictionary dico, string filename, string sectionName)
        {
            XmlTextWriter writer = null;
            try
            {
                writer = new XmlTextWriter(filename, Encoding.UTF8) { Formatting = Formatting.Indented };
                writer.WriteStartDocument(true);
                writer.WriteStartElement(sectionName);
                foreach (string key in dico.Keys)
                {
                    writer.WriteStartElement("add");
                    writer.WriteAttributeString("key", key);
                    writer.WriteAttributeString("value", (string)dico[key]);
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Flush();
                writer.Close();
            }
            catch (Exception e)
            {
                throw new Exception("Save Error - " + e.Message);
            }
            finally
            {
                if (writer != null)
                {
                    writer.Close();
                }
            }
        }

        /// <summary>
        /// comparaison de deux dictionnaires (string, string)
        /// </summary>
        /// <param name="origin">premier dictionnaire à comparer</param>
        /// <param name="target">second dictionnaire à comparer</param>
        /// <returns>true si les dictionnaires sont équivalents false sinon</returns>
        public static bool Compare(IDictionary origin, IDictionary target)
        {
            if ((target == null) || (origin == null))
            {
                return target == origin;
            }

            if (target.Count != origin.Count)
            {
                return false;
            }

            try
            {
                foreach (string key in origin.Keys)
                {
                    if ((string)origin[key] != (string)target[key])
                    {
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
        #region Méthodes Privées

        /// <summary>
        /// Deserialisation XML d'un IDictionary
        /// </summary>
        /// <param name="elementName">section xml</param>
        /// <param name="dicoFile">document xml</param>
        /// <returns>le IDictionary ou null en cas d'echec</returns>
        private static IDictionary GetConfig(string elementName, XmlDocument dicoFile)
        {
            if (dicoFile == null)
            {
                return null;
            }

            try
            {
                XmlNodeList nodes = dicoFile.GetElementsByTagName(elementName);

                foreach (XmlNode node in nodes)
                {
                    if (node.LocalName == elementName)
                    {
                        DictionarySectionHandler handler = new DictionarySectionHandler();
                        return (IDictionary)handler.Create(null, null, node);
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }

            return null;
        }

        #endregion
    }
}
