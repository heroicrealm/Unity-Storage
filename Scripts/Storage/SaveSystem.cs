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
     Основной класс для сохраниния.
     Создается как ScriptableObject Asset в рамках проекта
     */
    [CreateAssetMenu(fileName = "NewSaveSystem", menuName = "Save System", order = 51)]
    public class SaveSystem : ScriptableObject
    {
        /////////////////////////////////////////////////////////////////
        // CLASS MEMBERS
        /////////////////////////////////////////////////////////////////
    
        //Список отслеживаемых сущностей
        [System.NonSerialized]
        List<GameObject> monitoredObjects = new List<GameObject>();

        //Хранилище данных для хранения в памяти 
        [System.NonSerialized]
        Dictionary<string,List<SavePackage>> inMemorySave = new Dictionary<string,List<SavePackage>>();

        //Данные в свободном виде
        [System.NonSerialized]
        Dictionary<string,object> customData= new Dictionary<string,object>();

        //Метка не выгружать объект если неиспользется        
        private void OnEnable() => hideFlags = HideFlags.DontUnloadUnusedAsset;
        
        /////////////////////////////////////////////////////////////////////
        // PUBLIC METHODS
        /////////////////////////////////////////////////////////////////////
        
        //Добавляет сущность для отслеживания
        public void AddSaveableObject(GameObject gameObject)
        {                    
            monitoredObjects.Add(gameObject);                   
        }
        /**********************************************************************************************/
        //Удаляет сущность из списка отслеживаемых
        public void RemoveSaveableObject(GameObject gameObject)
        {
            this.monitoredObjects.Remove(gameObject);
        }
        /**********************************************************************************************/
        //Добавляет данные в свободном виде
        // Данные должны быть мериализуемы
        //
        public void SetСustomData(String key, object value)
        {
            this.customData[key] = value;
        }
        /**********************************************************************************************/
        //получает данные в свободном виде
        public object GetСustomData(String key)
        {
            return this.customData[key];            
        }
        /**********************************************************************************************/
        //Удаляет данные в свободном виде
        public void DeleteCustomData(String key)
        {
            this.customData.Remove(key);
        }
        /**********************************************************************************************/
        //Удаляет данные в свободном виде
        public bool ContainsCustomData(String key)
        {
            return this.customData.ContainsKey(key);
        }
        /**********************************************************************************************/

        //Сохраняет сущности в фаил
        public void Save(String fileName)
        {
            //Откроем фаил
            using (var fileStream = new FileStream(fileName, FileMode.OpenOrCreate))
            {
                using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Update))
                {
                    //сохраняем объекты сцены
                    var entry = archive.GetEntry(SceneManager.GetActiveScene().name + ".scd");
                    //удаляем прошлое сохранение
                    if (entry != null) entry.Delete();                    
                    var realmFile = archive.CreateEntry(SceneManager.GetActiveScene().name + ".scd");
                    using (var entryStream = realmFile.Open())
                    {
                        using (var streamWriter = new StreamWriter(entryStream))
                        {
                            //Сохраняем пакеты данных сцены как JSON фаил
                            streamWriter.Write(JsonConvert.SerializeObject(getSceneData(true), Formatting.Indented));
                        }
                    }

                    //Сохраняем глобальные данные
                    entry = archive.GetEntry("global.scd");
                    if (entry != null) entry.Delete();
                    realmFile = archive.CreateEntry("global.scd");
                    using (var entryStream = realmFile.Open())
                    using (var streamWriter = new StreamWriter(entryStream))
                    {
                        streamWriter.Write(JsonConvert.SerializeObject(getSceneData(false), Formatting.Indented));
                    }

                    //Сохраняем данные в свободном виде
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
        // Загружает сущности из фаила
        public void Load(string filename)
        {
            //Удаляем динамически созданные сущности
            DestroySpawned();
            //открываем архив
            using (var fileStream = new FileStream(filename, FileMode.Open))
            {
                using (var archive = new ZipArchive(fileStream))
                {
                    //Загружаем сущности сцены
                    var entry = archive.GetEntry(SceneManager.GetActiveScene().name + ".scd");
                    Debug.Log("Loading:" + entry.Name);
                    using (var entryStream = entry.Open())
                    using (var streamReader = new StreamReader(entryStream))
                    {
                        
                        loadSceneData(JsonConvert.DeserializeObject<List<SavePackage>>(streamReader.ReadToEnd()));
                    }

                    //Загружаем глобальные сущности
                    entry = archive.GetEntry("global.scd");
                    Debug.Log("Loading:" + entry.Name);
                    using (var entryStream = entry.Open())
                    using (var streamReader = new StreamReader(entryStream))
                    {
                        loadSceneData(JsonConvert.DeserializeObject<List<SavePackage>>(streamReader.ReadToEnd()));
                    }
                    //загружаем данные в свободном виде
                    entry = archive.GetEntry("custom.scd");
                  
                    using (var entryStream = entry.Open())
                    using (var streamReader = new StreamReader(entryStream))
                    {
                        loadCustomData(JsonConvert.DeserializeObject<List<SavePackage>>(streamReader.ReadToEnd()));
                    }


                }
            }
            //Сообщаем сущностям что данные загружены       
            foreach (GameObject go in this.monitoredObjects)
            {
                go.GetComponent<SaveableBehaviour>().OnWorldLoaded();
            }

        }
        
        /**********************************************************************************************/
        //Сохраняет данные в память        
        public void Save()
        {
            FlushInMemory();            
            inMemorySave.Add(SceneManager.GetActiveScene().name, getSceneData(true));
            inMemorySave.Add("__GLOBAL__",getSceneData(false));
            inMemorySave.Add("__CUSTOM__", getCustomData());
        }

        /***********************************************************************************************/
        //Загружает данные из памяти
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

            //Сообщаем сущностям что данные загружены       
            foreach (GameObject go in this.monitoredObjects)
            {
                go.GetComponent<SaveableBehaviour>().OnWorldLoaded();
            }
        }

        /**********************************************************************************************/
        //Очищает буффер в памяти
        public void FlushInMemory()
        {
            inMemorySave.Remove(SceneManager.GetActiveScene().name);
            inMemorySave.Remove("__GLOBAL__");
            inMemorySave.Remove("__CUSTOM__");
        }
        
        /**********************************************************************************************/
        //Очищает буфер памяти      

        /////////////////////////////////////////////////////////////////////
        // PRIVATE METHODS
        /////////////////////////////////////////////////////////////////////
        
        //  Удаляет ранее загруженные динамические объекты
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
        //Получения пакетов сохранения данных в свободном виде
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
        //Восстановление данных в свободном виде
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
        //Загрузка данных сцены
        private void loadSceneData(List<SavePackage> sceneData)
        {          
            foreach(SavePackage savePackage in sceneData)
            {
                if(savePackage.savebleType == ESavebleType.STATIC || savePackage.savebleType == ESavebleType.GLOBAL_STATIC) 
                {
                    loadStatic(savePackage); // модифифкация объектов созданных в редакторе юнити
                }
                else
                {
                    loadDynamic(savePackage); // спавн динамических объектов
                }
            }

        }
        /**********************************************************************************************/
        //спавн динамических объектов из пакета
        private void loadDynamic(SavePackage savePackage)
        {
            //Загружаем префаб
            GameObject prefab = Resources.Load(savePackage.prefabName) as GameObject;
            //Инстанцируем объект
            GameObject go = GameObject.Instantiate(prefab);
            //Настраиваем
            //Задаем настройки из пакета
            PopulateData(savePackage, go);
        }
                

        /**********************************************************************************************/
        //Загрузка статических объектов из пакета
        private void loadStatic(SavePackage savePackage)
        {
            //находим статический объект по имени
            GameObject go = monitoredObjects.Find((x)=>x.name == savePackage.name);
            //восстанавливаем из пакета
            PopulateData(savePackage, go);
        }

        /**********************************************************************************************/
        //Загрузка данных из пакета в объект
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
        //Получение данных сцены
        // Параметр saveScene - true - получить данные объектов сцены, false - глобальных объектов
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