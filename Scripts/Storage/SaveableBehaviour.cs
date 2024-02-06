
using System;
using UnityEngine;

namespace HeroicRealm.SaveSystem
{
    /**
     * Базовый класс для сохраняемых сущностей
     * Сохраняемая сущность должна предоставить:
     * string GetPrefabName()  - полное имя префаба. Для сущностей созданных в редакторе юнити  возвращаемое значение не анализируется
     * string SerializeObject() - Представление сущности в виде строки ( JSON/XML/свободный вид)
     * void loadData(string packageData) - Восстановление параметров сущности из строки
     * OnWorldLoaded() - метод вызываетcя для каждой сохраненной сущности после загрузки всех сущностей
     * Каждая сущность должна содержать ссылку на ScriptableObject SaveSystem
     */


    public abstract class SaveableBehaviour : MonoBehaviour
    {
        ////////////////////////////////////////////////////////        
        // CLASS MEMBERS
        ////////////////////////////////////////////////////////

        // Ссылка на систему сохранения
        [SerializeField]
        SaveSystem saveSystem;
        // Тип сущности
        [SerializeField]
        ESavebleType savebleType;

        
        /////////////////////////////////////////////////////////
        // Get/ Set
        /////////////////////////////////////////////////////////

        public void SetSaveSystem(SaveSystem saveSystem)
        {
            this.saveSystem = saveSystem;
        }

        public ESavebleType GetSavebleType() { return savebleType; }
        public void SetSavebleType(ESavebleType savebleType) { this.savebleType = savebleType; }
        
        
        /////////////////////////////////////////////////////////
        // CLASS METHODS
        ////////////////////////////////////////////////////////
        
        // Регистрирует сущность в системе сохранения
        // если сущность использует метод Awake она должна вызвать метод родителя
        protected void Awake()
        {   
            if (saveSystem != null) { }
                RegisterInSaveSystem();
        }
        /********************************************************************/
        //Регистрация в системе сохранения
        public void RegisterInSaveSystem()
        {
            saveSystem.AddSaveableObject(this.gameObject);
        }
        /********************************************************************/
        //Снятие регистрации в системе сохранения
        public void UnregisterInSaveSystem()
        {
            saveSystem.RemoveSaveableObject(this.gameObject);
        }        
        /********************************************************************/
        // Отменяет регистрацию сущности в системе сохранения
        // Если сущность использует метод OnDestroy она должна вызвать метод родителя
        protected void OnDestroy()
        {            
            UnregisterInSaveSystem();
        }


        /********************************************************************/
        // Служебный метод. Формирует пакет данных для системы сохранения
        public SavePackage GetSavePackage()
        {
            SavePackage package = new SavePackage();
            package.name = this.name;
            package.savebleType = this.savebleType;
            package.prefabName = this.GetPrefabName();
            //Кодируем данные сущности в Base64
            package.packageData = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(this.SerializeObject()));          
            return package;
        }        
        
        /////////////////////////////////////////////////////
        // ABSTRACT METHODS TO BE IMPLEMENDED
        /////////////////////////////////////////////////////
        
        //Вызывается когда все сущности загружены
        //Может быть использовано для восстановления ссылок между сущностями
        public void OnWorldLoaded() { }
        
        //Возвращает имя Префаба.
        public abstract string GetPrefabName();
        //Возвращает данные сущности в виде строки( JSON/XML/свободный вид)
        public abstract string SerializeObject();
        //Загружает данные сущности из строки
        public abstract void loadData(string packageData);
        
    }
}