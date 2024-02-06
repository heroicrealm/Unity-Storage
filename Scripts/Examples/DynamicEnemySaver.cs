using HeroicRealm.SaveSystem;
using Newtonsoft.Json;
using System;
using UnityEngine;

//������ ����������� ��������, ������� ��������� �� ����� ����
//����� �������� �����������, ��������� ���� ������ � �������
//� ���������� ������� ���������� ������ �� ScriptableObject SaveSystem
//� ���������� ������� ���������� ��� ������� DYNAMIC ��� GLOBAL_DYNAMIC
//��� �� ��� ������ ����� ���������� ����� GameObject.Instantiate
public class DynamicEnemySaver : SaveableBehaviour
{
    
    SpriteRenderer spriteRenderer;


    public override string GetPrefabName()
    {       
        return "DynamicEnemy";
    }

    new private void Awake()
    {
        base.Awake();
        spriteRenderer = GetComponent<SpriteRenderer>();        
    }
    //�������������� ������
    public override void loadData(string packageData)
    {
       
        _SESData ssd = JsonConvert.DeserializeObject<_SESData>(packageData);
        this.transform.position = new Vector3(ssd.x, ssd.y, 0);
        spriteRenderer.color = new Color(ssd.r,ssd.g,ssd.b);
    }

    public override string SerializeObject()
    {
        _SESData ssd = new _SESData();
        ssd.x = this.transform.position.x;
        ssd.y = this.transform.position.y;
        ssd.r = spriteRenderer.color.r;
        ssd.g = spriteRenderer.color.g;
        ssd.b = spriteRenderer.color.b;
        return JsonConvert.SerializeObject(ssd);
    }

    public void SetColor(Color col)
    {
        this.spriteRenderer.color = col;
    }

    [Serializable]
    private struct _SESData
    {
        public float x;
        public float y;
        public float r;
        public float g;
        public float b;
    }
}
