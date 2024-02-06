using HeroicRealm.SaveSystem;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//������ ����������� ��������, ������� ��������� � ��������� �����
//����� �������� �����������, ��������� ���� ������ � ������� ����� �����
//� ���������� ���������� ������ �� ScriptableObject SaveSystem
//� ���������� ���������� ��� ������� STATIC ��� GLOBAL_STATIOC
//!!!��� ������� ������ ���� ���������� � ������ �����!!!
public class StaticEnemySaver : SaveableBehaviour
{
    
    public override string GetPrefabName()
    {
        return "StaticEnemy";
    }
    //�������������� ������
    public override void loadData(string packageData)
    {
        //�������������� �� JSON
        _SESData ssd = JsonConvert.DeserializeObject<_SESData>(packageData);
        //��������������� ������, ������ ������ �������
        this.transform.position = new Vector3(ssd.x, ssd.y, 0);
    }

    //���������� ������
    public override string SerializeObject()
    {
        //��������� � ��������� �����
        _SESData ssd = new _SESData();
        ssd.x = this.transform.position.x;
        ssd.y = this.transform.position.y;   

        //����������� � JSON
        return JsonConvert.SerializeObject(ssd);

    }

    
    [Serializable]
    private struct _SESData
    {
       public float x;
       public float y;
    }
}
