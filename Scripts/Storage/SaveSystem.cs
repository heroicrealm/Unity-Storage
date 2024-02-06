using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace HeroicRealm.SaveSystem
{

    /**
     �������� ����� ��� ����������.
     ��������� ��� ScriptableObject Asset � ������ �������
     */
    [CreateAssetMenu(fileName = "NewSaveSystem", menuName = "Save System", order = 51)]
    public class SaveSystem : ScriptableObject
    {
        /////////////////////////////////////////////////////////////////
        // CLASS MEMBERS
        /////////////////////////////////////////////////////////////////
    
        //������ ������������� ���������
        [System.NonSerialized]
        List<GameObject> monitoredObjects = new List<GameObject>();

        //��������� ������ ��� �������� � ������ 
        [System.NonSerialized]
        Dictionary<string,List<SavePackage>> inMemorySave = new Dictionary<string,List<SavePackage>>();

        //������ � ��������� ����
        [System.NonSerialized]
        Dictionary<string,object> customData= new Dictionary<string,object>();

        //����� �� ��������� ������ ���� �������������        
        private void OnEnable() => hideFlags = HideFlags.DontUnloadUnusedAsset;
        
        /////////////////////////////////////////////////////////////////////
        // PUBLIC METHODS
        /////////////////////////////////////////////////////////////////////
        
        //��������� �������� ��� ������������
        public void AddSaveableObject(GameObject gameObject)
        {                    
            monitoredObjects.Add(gameObject);                   
        }
        /**********************************************************************************************/
        //������� �������� �� ������ �������������
        public void RemoveSaveableObject(GameObject gameObject)
        {
            this.monitoredObjects.Remove(gameObject);
        }
        /**********************************************************************************************/
        //��������� ������ � ��������� ����
        // ������ ������ ���� ������������
        //
        public void Set�ustomData(String key, object value)
        {
            this.customData[key] = value;
        }
        /**********************************************************************************************/
        //�������� ������ � ��������� ����
        public object Get�ustomData(String key)
        {
            return this.customData[key];            
        }
        /**********************************************************************************************/
        //������� ������ � ��������� ����
        public void DeleteCustomData(String key)
        {
            this.customData.Remove(key);
        }
        /**********************************************************************************************/
        //������� ������ � ��������� ����
        public bool ContainsCustomData(String key)
        {
            return this.customData.ContainsKey(key);
        }
        /**********************************************************************************************/

        //��������� �������� � ����
        public void Save(String fileName)
        {
            //������� ����
            using (var fileStream = new FileStream(fileName, FileMode.OpenOrCreate))
            {
                using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Update))
                {
                    //��������� ������� �����
                    var entry = archive.GetEntry(SceneManager.GetActiveScene().name + ".scd");
                    //������� ������� ����������
                    if (entry != null) entry.Delete();                    
                    var realmFile = archive.CreateEntry(SceneManager.GetActiveScene().name + ".scd");
                    using (var entryStream = realmFile.Open())
                    {
                        using (var streamWriter = new StreamWriter(entryStream))
                        {
                            //��������� ������ ������ ����� ��� JSON ����
                            streamWriter.Write(JsonConvert.SerializeObject(getSceneData(true), Formatting.Indented));
                        }
                    }

                    //��������� ���������� ������
                    entry = archive.GetEntry("global.scd");
                    if (entry != null) entry.Delete();
                    realmFile = archive.CreateEntry("global.scd");
                    using (var entryStream = realmFile.Open())
                    using (var streamWriter = new StreamWriter(entryStream))
                    {
                        streamWriter.Write(JsonConvert.SerializeObject(getSceneData(false), Formatting.Indented));
                    }

                    //��������� ������ � ��������� ����
                    entry = archive.GetEntry("custom.scd");
                    if (entry != null) entry.Delete();
                    realmFile = archive.CreateEntry("custom.scd");
                    using (var entryStream = realmFile.Open())
                    using (var streamWriter = new StreamWriter(entryStream))
                    {
                        streamWriter.Write(JsonConvert.SerializeObject(getCustomData(), Formatting.Indented));
                    }
                }
            }
        }

        /**********************************************************************************************/
        // ��������� �������� �� �����
        public void Load(string filename)
        {
            //������� ����������� ��������� ��������
            DestroySpawned();
            //��������� �����
            using (var fileStream = new FileStream(filename, FileMode.Open))
            {
                using (var archive = new ZipArchive(fileStream))
                {
                    //��������� �������� �����
                    var entry = archive.GetEntry(SceneManager.GetActiveScene().name + ".scd");
                    Debug.Log("Loading:" + entry.Name);
                    using (var entryStream = entry.Open())
                    using (var streamReader = new StreamReader(entryStream))
                    {
                        
                        loadSceneData(JsonConvert.DeserializeObject<List<SavePackage>>(streamReader.ReadToEnd()));
                    }

                    //��������� ���������� ��������
                    entry = archive.GetEntry("global.scd");
                    Debug.Log("Loading:" + entry.Name);
                    using (var entryStream = entry.Open())
                    using (var streamReader = new StreamReader(entryStream))
                    {
                        loadSceneData(JsonConvert.DeserializeObject<List<SavePackage>>(streamReader.ReadToEnd()));
                    }
                    //��������� ������ � ��������� ����
                    entry = archive.GetEntry("custom.scd");
                  
                    using (var entryStream = entry.Open())
                    using (var streamReader = new StreamReader(entryStream))
                    {
                        loadCustomData(JsonConvert.DeserializeObject<List<SavePackage>>(streamReader.ReadToEnd()));
                    }


                }
            }
            //�������� ��������� ��� ������ ���������       
            foreach (GameObject go in this.monitoredObjects)
            {
                go.GetComponent<SaveableBehaviour>().OnWorldLoaded();
            }

        }
        
        /**********************************************************************************************/
        //��������� ������ � ������        
        public void Save()
        {
            FlushInMemory();            
            inMemorySave.Add(SceneManager.GetActiveScene().name, getSceneData(true));
            inMemorySave.Add("__GLOBAL__",getSceneData(false));
            inMemorySave.Add("__CUSTOM__", getCustomData());
        }

        /***********************************************************************************************/
        //��������� ������ �� ������
        public void Load()
        {
            DestroySpawned();
            List<SavePackage> list;

            if (inMemorySave.TryGetValue(SceneManager.GetActiveScene().name, out list))
            {               
                loadSceneData(list);
            }
            if (inMemorySave.TryGetValue("__GLOBAL__", out list))
            {
                loadSceneData(list);
            }
            if (inMemorySave.TryGetValue("__CUSTOM__", out list))
            {
                loadCustomData(list);
            }

            //�������� ��������� ��� ������ ���������       
            foreach (GameObject go in this.monitoredObjects)
            {
                go.GetComponent<SaveableBehaviour>().OnWorldLoaded();
            }
        }

        /**********************************************************************************************/
        //������� ������ � ������
        public void FlushInMemory()
        {
            inMemorySave.Remove(SceneManager.GetActiveScene().name);
            inMemorySave.Remove("__GLOBAL__");
            inMemorySave.Remove("__CUSTOM__");
        }
        
        /**********************************************************************************************/
        //������� ����� ������      

        /////////////////////////////////////////////////////////////////////
        // PRIVATE METHODS
        /////////////////////////////////////////////////////////////////////
        
        //  ������� ����� ����������� ������������ �������
        private void DestroySpawned()
        {
            foreach (GameObject go in monitoredObjects)
            {
                SaveableBehaviour sb = go.GetComponent<SaveableBehaviour>();
                if(sb.GetSavebleType() == ESavebleType.DYNAMIC ||
                    sb.GetSavebleType() == ESavebleType.GLOBAL_DYNAMIC)
                { 
                    Destroy(go);
                }
            }
            
            monitoredObjects.RemoveAll((x) => {
                return x.GetComponent<SaveableBehaviour>().GetSavebleType() == ESavebleType.DYNAMIC || x.GetComponent<SaveableBehaviour>().GetSavebleType() == ESavebleType.GLOBAL_DYNAMIC;
                });         
        }

        /**********************************************************************************************/
        //��������� ������� ���������� ������ � ��������� ����
        private List<SavePackage> getCustomData()
        {
            List<SavePackage> packs = new List<SavePackage>();
            foreach(string key in this.customData.Keys)
            {
                SavePackage pack = new SavePackage();
                pack.name = key;
                pack.savebleType = ESavebleType.SETTINGS;
                pack.prefabName = this.customData[key].GetType().FullName;
                pack.packageData = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this.customData[key])));
                packs.Add(pack);
            }
            return packs;
        }
        /**********************************************************************************************/
        //�������������� ������ � ��������� ����
        public void loadCustomData(List<SavePackage> packages)
        {
            foreach (SavePackage pack in packages)
            {
                byte[] bDtata = Convert.FromBase64String(pack.packageData);
                string json = System.Text.Encoding.UTF8.GetString(bDtata);
                customData[pack.name] = JsonConvert.DeserializeObject(json, Type.GetType(pack.prefabName));
            }
        }


        /**********************************************************************************************/
        //�������� ������ �����
        private void loadSceneData(List<SavePackage> sceneData)
        {          
            foreach(SavePackage savePackage in sceneData)
            {
                if(savePackage.savebleType == ESavebleType.STATIC || savePackage.savebleType == ESavebleType.GLOBAL_STATIC) 
                {
                    loadStatic(savePackage); // ������������ �������� ��������� � ��������� �����
                }
                else
                {
                    loadDynamic(savePackage); // ����� ������������ ��������
                }
            }

        }
        /**********************************************************************************************/
        //����� ������������ �������� �� ������
        private void loadDynamic(SavePackage savePackage)
        {
            //��������� ������
            GameObject prefab = Resources.Load(savePackage.prefabName) as GameObject;
            //������������ ������
            GameObject go = GameObject.Instantiate(prefab);
            //�����������
            //������ ��������� �� ������
            PopulateData(savePackage, go);
        }
                

        /**********************************************************************************************/
        //�������� ����������� �������� �� ������
        private void loadStatic(SavePackage savePackage)
        {
            //������� ����������� ������ �� �����
            GameObject go = monitoredObjects.Find((x)=>x.name == savePackage.name);
            //��������������� �� ������
            PopulateData(savePackage, go);
        }

        /**********************************************************************************************/
        //�������� ������ �� ������ � ������
        private void PopulateData(SavePackage savePackage, GameObject go)
        {
            byte[] bDtata = Convert.FromBase64String(savePackage.packageData);
            string json = System.Text.Encoding.UTF8.GetString(bDtata);
            SaveableBehaviour sb = go.GetComponent<SaveableBehaviour>();
            sb.SetSaveSystem(this);
            sb.SetSavebleType(savePackage.savebleType);
            sb.name = savePackage.name;
            sb.loadData(json);
        }
        /**********************************************************************************************/
        //��������� ������ �����
        // �������� saveScene - true - �������� ������ �������� �����, false - ���������� ��������
        private List<SavePackage> getSceneData(bool saveScene)
        {
            List<SavePackage> sceneData = new List<SavePackage>();
           

            foreach(GameObject go in monitoredObjects) {
                SaveableBehaviour sb = go.GetComponent<SaveableBehaviour>();
                if (saveScene && ( sb.GetSavebleType() == ESavebleType.STATIC || sb.GetSavebleType() == ESavebleType.DYNAMIC) )
                {
                    sceneData.Add(sb.GetSavePackage());
                }
                if (!saveScene && (sb.GetSavebleType() == ESavebleType.GLOBAL_STATIC || sb.GetSavebleType() == ESavebleType.GLOBAL_DYNAMIC))
                {
                    sceneData.Add(sb.GetSavePackage());
                }

            }

            return sceneData;
           
        }


      
    }
}