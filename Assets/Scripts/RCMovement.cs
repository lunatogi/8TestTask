using UnityEngine;
using DG.Tweening;
using System.Collections;
using System;
using Unity.VisualScripting;
using System.Collections.Generic;

public class RCMovement : MonoBehaviour
{
    public bool move;
    public bool collectPath;

    Ray camRay;
    RaycastHit camHit;
    [SerializeField] private Transform objectHit;
    [SerializeField] private Transform mousePosition;

    //[SerializeField] private Transform currentGrid;
    private Transform nextGrid;                     // In order to detect grid change

    public List<Vector3> movePath;
    public List<Vector3> currentMovePath;
    private List<Transform> independentWegans;

    public Vector3[] movePathArray;             // ConvertArrayListToVectorArray metodunun içine al

    int speed;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        speed = 5;
        movePath = new List<Vector3>();
        independentWegans = new List<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        PathRay();


        if(Application.isMobilePlatform){

            if(Input.touchCount > 0){
                Touch touch = Input.GetTouch(0);

                if(touch.phase == TouchPhase.Began){
                    if(mousePosition.gameObject.tag == "RC"){
                        //move = true;
                        objectHit = mousePosition;                          // Assign new RC
                    }
                }

                if(touch.phase == TouchPhase.Ended){
                    ParentWegans();
                    Invoke("ParentWegans", 0.1f);                                       // Parent wegans again
                    //move = false;
                    objectHit = null;                                       // Free selected RC
                    independentWegans.Clear();                            // Clear ArrayList
                    if(movePath != null)
                        movePath.Clear();                                   // Free path arraylist
                }
            }
        }else{
            if(Input.GetMouseButtonDown(0)){
                if(mousePosition.gameObject.tag == "RC"){
                    //move = true;
                    objectHit = mousePosition;                          // Assign new RC
                }
            }

            if(Input.GetMouseButtonUp(0)){
                ParentWegans();
                Invoke("ParentWegans", 0.1f);                                       // Parent wegans again
                //move = false;
                objectHit = null;                                       // Free selected RC
                independentWegans.Clear();                            // Clear ArrayList
                if(movePath != null)
                    movePath.Clear();                                   // Free path arraylist
            }
        }



        if(movePath != null && objectHit != null){
            DeparentWegans();
            Invoke("MoveRC", 0.1f);
        }
    }

    private void PathRay(){

        Vector3 inputPos;

        if (Application.isMobilePlatform)
        {
            if (Input.touchCount == 0) return;

            Touch touch = Input.GetTouch(0);
            if (touch.phase != TouchPhase.Began && touch.phase != TouchPhase.Moved) return;

            inputPos = touch.position;
        }
        else
        {
            inputPos = Input.mousePosition;
        }

        camRay = Camera.main.ScreenPointToRay(inputPos);

        if(Physics.Raycast(camRay, out camHit)){
            mousePosition = camHit.transform;                       // Get mouse position 
            if(camHit.transform.gameObject.tag == "Grid"){          // BURAYA MOUSE GRIDE BASMAZSA DA EN YAKIN GRIDI ALMAYI EKLE
                nextGrid = camHit.transform;                        // Get the grid to move on
                if(objectHit != null){
                    if(movePath == null){
                        AddGridToPath();
                    }
                    else if(!movePath.Contains(nextGrid.position)){
                        AddGridToPath();
                    }
                    //move = true;
                }
            }
        }
    }

    private void MoveRC(){
        //currentGrid = nextGrid;
        //move = false;
        currentMovePath = new List<Vector3>(movePath);
        movePath.Clear();
        int distance = movePath.Count;
        if(distance == 0) distance = 1;
        float duration = (float)distance / speed;       // For it move with same velocity
        objectHit.DOPath(ConvertArrayListToVectorArray(currentMovePath), duration).SetLookAt(0.005f);
        //MoveWegans(duration);
        /*
        if(movePath != null && movePath.Count > 0){
            Vector3 nextMovement = movePath[gridLoc];
            objectHit.DOLookAt(nextMovement, 0.1f);
            objectHit.DOMove(nextMovement, 3f);
            //movePath.RemoveAt(0);
            //
            gridLoc++;
            move = false;
        }
        */
    }

    private void MoveWegans(float _duration){
        List<Vector3> weganPath = new List<Vector3>(currentMovePath);
        weganPath.RemoveAt(weganPath.Count - 1);
        weganPath.Add(UpdateLastLocation(weganPath[0], weganPath[weganPath.Count-1]));
        foreach(Transform wegan in independentWegans){
            Vector3 currentWeganPosition = new Vector3(wegan.position.x, wegan.position.y, wegan.position.z);
            wegan.DOPath(ConvertArrayListToVectorArray(weganPath), _duration).SetLookAt(0.005f).SetDelay(0.01f);
            weganPath.Insert(0, currentWeganPosition);
            weganPath.RemoveAt(weganPath.Count - 1);
        }
    }

    private Vector3 UpdateLastLocation(Vector3 _startPos, Vector3 _endPos){
        Vector3 _updatedLoc;
        Vector3 direction = (_endPos - _startPos).normalized;
        _updatedLoc = _endPos - direction;
        return _updatedLoc;
    }


    private void AddGridToPath(){

        Vector3 lastGrid;
        lastGrid = CalculateFrom();
        
        float deltaX = lastGrid.x - nextGrid.position.x;
        float deltaZ = lastGrid.z - nextGrid.position.z;

        if(CheckForDiagonality(deltaX, deltaZ)){
            if(NeighbourAvailability(nextGrid.position.x, nextGrid.position.z)){
                movePath.Add(nextGrid.position);
            }
        }else{
            movePath.Add(nextGrid.position);
        }
        
    }

    private bool CheckForDiagonality(float x, float z){
        bool diagonal = false;
        if(x != 0 && z != 0){
            return true;
        }
        return diagonal;
    }

/*
    private bool CheckForDistance(float x, float z){
        bool soFar = false;
        if(Mathf.Abs(x) > 1 || Mathf.Abs(z) > 1){
            soFar = true;
        }
        return soFar;
    }
*/

    private Vector3 CalculateFrom(){
        Vector3 lastGrid;
        if(movePath == null || movePath.Count < 1){
            lastGrid = objectHit.position;
        }else{
            lastGrid = movePath[movePath.Count-1];
        }
        return lastGrid;
    }

    private bool NeighbourAvailability(float x, float z){
        bool neighbourAdded = false;

        Vector3 lastPos;
        lastPos = CalculateFrom();
        
        Vector3 targetPosition = new Vector3(x, lastPos.y, lastPos.z);
        GameObject _GameObject = GetGameObjectInPosition(targetPosition);
        if(_GameObject.tag == "Grid"){
            movePath.Add(_GameObject.transform.position);
            neighbourAdded = true;
        }else{
            targetPosition = new Vector3(lastPos.x, lastPos.y, z);
            _GameObject = GetGameObjectInPosition(targetPosition);
            if(_GameObject.tag == "Grid"){
                movePath.Add(_GameObject.transform.position);
                neighbourAdded = true;
            }
        }
        return neighbourAdded;
    }

    private GameObject GetGameObjectInPosition(Vector3 _targetPosition){
        Camera mainCamera = Camera.main;
        Ray ray = new Ray(mainCamera.transform.position, (_targetPosition - mainCamera.transform.position).normalized);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.collider.gameObject;
        } else return null;
        
    }

    private void DeparentWegans(){
        
        foreach (Transform wegan in objectHit)
        {
            if(wegan.gameObject.tag != "Head"){
                independentWegans.Add(wegan);
                wegan.SetParent(null);
            }
        }
    }

    private Vector3 ClosestIntegerOfAPosition(Vector3 _position){
        return new Vector3((float)Math.Round(_position.x), _position.y, (float)Math.Round(_position.z));
    }

    private void ParentWegans(){
        foreach(Transform wegan in independentWegans){
            wegan.SetParent(objectHit);
        }
    }

    private Vector3[] ConvertArrayListToVectorArray(List<Vector3> _arrayList){
        movePathArray = new Vector3[_arrayList.Count];
        int vecCounter = 0;
        foreach(Vector3 vec in _arrayList){
            if(vec != null){
                movePathArray[vecCounter] = vec;
                vecCounter++;
            }
        }
        return movePathArray;
    }

/*

    public void RemoveGridFromPath(Vector3 _gridVector){
        // ArrayList updatedPath = new ArrayList();
        // foreach(Vector3 grid in movePath){
        //     if(grid != _gridVector){
        //         updatedPath.Add(grid);
        //     }
        // }
        // movePath = updatedPath;

        if(movePath.Contains(_gridVector)) movePath.Remove(_gridVector);
    }
*/
}
