using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeroicRealm.SaveSystem
{
   /**
    * Служебный класс для сохранения данных о сущности
    * */
    [Serializable]
    public class SavePackage 
    {
        public ESavebleType savebleType; //тип сущности
        public string name;  // имя сущности
        public string prefabName; // имя префаба
        public string packageData; // Данные сущности
    }
}
