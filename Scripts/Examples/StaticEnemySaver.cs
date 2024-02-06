using HeroicRealm.SaveSystem;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Пример сохраняемой сущности, которая создается в редакторе юнити
//Чтобы сущность сохранялась, прицепить этот скрипт к объекту сцены юнити
//В инспекторе установить ссылку на ScriptableObject SaveSystem
//В инспекторе установить тип объекта STATIC или GLOBAL_STATIOC
//!!!Имя объекта должно быть уникальным в рамках сцены!!!
public class StaticEnemySaver : SaveableBehaviour
{
    
    public override string GetPrefabName()
    {
        return "StaticEnemy";
    }
    //Восстановление данных
    public override void loadData(string packageData)
    {
        //десериализация из JSON
        _SESData ssd = JsonConvert.DeserializeObject<_SESData>(packageData);
        //Восстанавливаем данные, храним только позицию
        this.transform.position = new Vector3(ssd.x, ssd.y, 0);
    }

    //Сохранение данных
    public override string SerializeObject()
    {
        //Сохраняем в служебный класс
        _SESData ssd = new _SESData();
        ssd.x = this.transform.position.x;
        ssd.y = this.transform.position.y;   

        //сериализуем в JSON
        return JsonConvert.SerializeObject(ssd);

    }

    
    [Serializable]
    private struct _SESData
    {
       public float x;
       public float y;
    }
}
