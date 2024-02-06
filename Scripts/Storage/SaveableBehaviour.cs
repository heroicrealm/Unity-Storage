
using System;
using UnityEngine;

namespace HeroicRealm.SaveSystem
{
    /**
     * ������� ����� ��� ����������� ���������
     * ����������� �������� ������ ������������:
     * string GetPrefabName()  - ������ ��� �������. ��� ��������� ��������� � ��������� �����  ������������ �������� �� �������������
     * string SerializeObject() - ������������� �������� � ���� ������ ( JSON/XML/��������� ���)
     * void loadData(string packageData) - �������������� ���������� �������� �� ������
     * OnWorldLoaded() - ����� ��������c� ��� ������ ����������� �������� ����� �������� ���� ���������
     * ������ �������� ������ ��������� ������ �� ScriptableObject SaveSystem
     */


    public abstract class SaveableBehaviour : MonoBehaviour
    {
        ////////////////////////////////////////////////////////        
        // CLASS MEMBERS
        ////////////////////////////////////////////////////////

        // ������ �� ������� ����������
        [SerializeField]
        SaveSystem saveSystem;
        // ��� ��������
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
        
        // ������������ �������� � ������� ����������
        // ���� �������� ���������� ����� Awake ��� ������ ������� ����� ��������
        protected void Awake()
        {   
            if (saveSystem != null) { }
                RegisterInSaveSystem();
        }
        /********************************************************************/
        //����������� � ������� ����������
        public void RegisterInSaveSystem()
        {
            saveSystem.AddSaveableObject(this.gameObject);
        }
        /********************************************************************/
        //������ ����������� � ������� ����������
        public void UnregisterInSaveSystem()
        {
            saveSystem.RemoveSaveableObject(this.gameObject);
        }        
        /********************************************************************/
        // �������� ����������� �������� � ������� ����������
        // ���� �������� ���������� ����� OnDestroy ��� ������ ������� ����� ��������
        protected void OnDestroy()
        {            
            UnregisterInSaveSystem();
        }


        /********************************************************************/
        // ��������� �����. ��������� ����� ������ ��� ������� ����������
        public SavePackage GetSavePackage()
        {
            SavePackage package = new SavePackage();
            package.name = this.name;
            package.savebleType = this.savebleType;
            package.prefabName = this.GetPrefabName();
            //�������� ������ �������� � Base64
            package.packageData = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(this.SerializeObject()));          
            return package;
        }        
        
        /////////////////////////////////////////////////////
        // ABSTRACT METHODS TO BE IMPLEMENDED
        /////////////////////////////////////////////////////
        
        //���������� ����� ��� �������� ���������
        //����� ���� ������������ ��� �������������� ������ ����� ����������
        public void OnWorldLoaded() { }
        
        //���������� ��� �������.
        public abstract string GetPrefabName();
        //���������� ������ �������� � ���� ������( JSON/XML/��������� ���)
        public abstract string SerializeObject();
        //��������� ������ �������� �� ������
        public abstract void loadData(string packageData);
        
    }
}