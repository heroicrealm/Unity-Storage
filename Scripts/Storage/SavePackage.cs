using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeroicRealm.SaveSystem
{
   /**
    * ��������� ����� ��� ���������� ������ � ��������
    * */
    [Serializable]
    public class SavePackage 
    {
        public ESavebleType savebleType; //��� ��������
        public string name;  // ��� ��������
        public string prefabName; // ��� �������
        public string packageData; // ������ ��������
    }
}
