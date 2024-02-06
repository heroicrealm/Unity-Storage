using HeroicRealm.SaveSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Пример использования системы сохранения
public class DemoBehaviour : MonoBehaviour
{
    [SerializeField]
    SaveSystem saveSystem;

    public void Start()
    {
        //Есть префаб "DynamicEnemy"
        GameObject pref = Resources.Load("DynamicEnemy") as GameObject;
        //Создадим 10 штук
        for (int i = 0; i <= 10; i++) {
            GameObject go = GameObject.Instantiate(pref);
            DynamicEnemySaver sb = go.GetComponent<DynamicEnemySaver>();

            sb.SetSavebleType(ESavebleType.DYNAMIC);

            sb.SetColor(new Color(
               Random.Range(0f, 1f),
               Random.Range(0f, 1f),
               Random.Range(0f, 1f)
              ));
            sb.SetSaveSystem(saveSystem); //Если в префабе проставлена ссылка на SaveSystem, данная строка не нужна
            sb.RegisterInSaveSystem(); //Если в префабе проставлена ссылка на SaveSystem, данная строка не нужна

            go.transform.position = new Vector2(0f, 0f) + Random.insideUnitCircle * 5;
            go.name = "MyEnemy" + go.GetInstanceID();
        }
        saveSystem.SetСustomData("Prop", "Hello");
    }


    public void Load()
    {
       saveSystem.Load("MySave.sav");
    }

    public void Save()
    {
        saveSystem.Load("MySave.sav");
    }



}
