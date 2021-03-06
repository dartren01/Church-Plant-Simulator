﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;


public class NPCSpawner : MonoBehaviour
{
    public static NPCSpawner current;
    //List of different NPCs to spawn. CANNOT BE NULL
    //Put all of the NPCs here from the inspector, or can add later in code
    public GameObject NPCObject;
    //multiply by list length to store that many NPCs
    public int startVisitorPoolAmount = 10;
    public int startMemberAmount = 3;
    public int memberCount = 3;

    [Header("NPC List")]
    //pool of NPCs to store in
    // linked list because we have a lot of insert and remove operations.
    public LinkedList<GameObject> MembersList = new LinkedList<GameObject>();
    public LinkedList<GameObject> VisitorsList = new LinkedList<GameObject>();

    // Start is called before the first frame update
    void Awake()
    {
        if (current == null)
            current = this;
        else
        {
            Destroy(this.gameObject);
            return;
        }
    }

    private void Start()
    {
        // set event values
        GameEvents.instance.onSundayEvent += ActivateMembers;

        //Initialize a set amount of each NPCs
        if (NPCObject == null)
        {
            Debug.LogError("NPC object empty. Fill with NPC.");
            return;
        }
        // set visitors
        for (int k = 0; k < startVisitorPoolAmount; k++)
        {
            GameObject obj = (GameObject)Instantiate(NPCObject);
            obj.SetActive(false);
            VisitorsList.AddLast(obj);
        }

        //set members
        for (int k = 0; k < startMemberAmount; k++)
        {
            GameObject obj = (GameObject)Instantiate(NPCObject);
            obj.SetActive(false);
            MembersList.AddLast(obj);
        }

        Debug.Log("All spawned");
    }
    // returns a disabled visitor npc from visitors list.
    public GameObject GetVisitorNPC()
    {
        foreach(GameObject npc in VisitorsList)
        {
            if (npc != null && !npc.activeInHierarchy)
            {
                npc.SetActive(true);
                return npc;
            }
        }
        return null;
    }

    // returns a disabled visitor npc from members list.
    public GameObject GetMemberNPC()
    {
        foreach (GameObject npc in MembersList)
        {
            if (npc != null && !npc.activeInHierarchy)
            {
                npc.SetActive(true);
                return npc;
            }
        }
        return null;
    }

    public int ReturnMemberCount()
    {
        return memberCount;
    }

    // sets all members active in heirarchy.
    public void ActivateMembers()
    {
        foreach (GameObject npc in MembersList)
        {
            if (npc != null && !npc.activeInHierarchy)
            {
                npc.SetActive(true);
            }
        }
    }

    // sets all npcs in the heirarchy to disabled
    public void DeactivateAll()
    {
        foreach (GameObject npc in MembersList)
        {
            if (npc != null && npc.activeInHierarchy)
            {
                npc.SetActive(false);
            }
        }
        foreach (GameObject npc in VisitorsList)
        {
            if (npc != null && npc.activeInHierarchy)
            {
                npc.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Pools more NPCs into the scene when there are not enough NPCs
    /// </summary>
    /// <param name="listIndex"></param>
    /// <returns>none</returns>
    public GameObject PoolMoreNPCs()
    {
        GameObject obj = (GameObject)Instantiate(NPCObject);
        obj.SetActive(true);
        VisitorsList.AddLast(obj);
        return obj;
    }


    /*
     * Calculate each visitor's chance to come back next sunday.
     * If the chance is successful, keep them in the visitors list,
     * then calculate member chance.
     * if member chance is successful, move them to members list
     * 
     * if not come back next sunday, remove them from the list.
     */

    public List<GameObject> CalculateVisitorSunday()
    {
        LinkedListNode<GameObject> head = VisitorsList.First;
        LinkedListNode<GameObject> temp = null;
        List<GameObject> newMembers = new List<GameObject>();
        while (head != null && head.Value.activeInHierarchy)
        {
            NPC npcScript = head.Value.GetComponent<NPC>();
            if (npcScript.CalculateComeBackSunday())
            {
                if (npcScript.CalculateMemberChance())
                {
                    temp = head;
                    head = head.Next;
                    newMembers.Add(temp.Value);
                    VisitorsList.Remove(temp);
                    MembersList.AddLast(temp);
                    memberCount++;
                    // add event that this npc became a member

                    continue;
                }
                head = head.Next;
            }
            else
            {
                temp = head;
                head = head.Next;
                VisitorsList.Remove(temp);
                VisitorsList.AddLast(temp);
                temp.Value.SetActive(true);
                temp.Value.GetComponent<NPC>().Shuffle();
                temp.Value.SetActive(false);
            }
        }
        return newMembers;
    }


    // maybe integrate these to string functions into creation of the npc
    // listnum 1 is visitor, evrything else is member
    public string NPCListToString(int listNum)
    {
        LinkedList<GameObject> npcList;
        if (listNum == 1)
            npcList = VisitorsList;
        else
            npcList = MembersList;
        StringBuilder list = new StringBuilder("", 100);
        foreach (GameObject npc in npcList)
        {
            if (npc != null && npc.activeInHierarchy)
            {
                list.AppendLine(npc.name);
            }
        }
        if (list.Length == 0)
            list.AppendLine("None");
        return list.ToString();
    }
}
