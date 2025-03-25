using DG.Tweening;
using UnityEngine;
using System.Collections.Generic;

public class WeganMovement : MonoBehaviour
{

    public List<Vector3> weganMovePathFromAhead;
    public List<Vector3> _weganMovePathFromAhead;
    public List<Vector3> weganMovePathCurrent;

    private Vector3[] movePathArray;
                                        

    public Transform currentGrid;
    public Transform lastGrid;

    public Transform weganAhead;

    // For another logic
    private Vector3 lastAheadPosition;

    public Vector3 backPosition;
    public Vector3 back;
    // -----------------


    public bool canMove;

    public int speed;

    public bool head;
    public bool last;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //currentGrid = transform;
        if(!head){
            HoldPassedGrids();
            back = -weganAhead.forward;
            backPosition = weganAhead.position + back;
            canMove = true;
            lastAheadPosition = weganAhead.position;
            speed = 20;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if(!last) HoldPassedGrids();

        if(!head) FollowWagen();
        
    }

/*
    private void FollowWagen(){
        if(weganAhead != null){
            back = -PositionRounder(weganAhead.forward);
            backPosition = weganAhead.position + back;   
            if(backPosition != transform.position && canMove){
 
                canMove = false;
                RotateWegan();
                transform.DOMove(backPosition, 0.2f).SetEase(Ease.Linear).OnComplete(() => canMove = true);
            }   
        }
    }

    private void RotateWegan(){
        Vector3 dir = (backPosition - transform.position).normalized;
        Vector3 snapDir = new Vector3(Mathf.Round(dir.x), 0, Mathf.Round(dir.z));

        if (snapDir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(snapDir);

    }

    
    private void FollowWagen()
    {
        if (weganAhead != null)
        {
            // Calculate direction based on last frame
            Vector3 movementDir = (weganAhead.position - lastAheadPosition).normalized;
            Vector3 gridDir = PositionRounder(movementDir);

            back = -gridDir;
            backPosition = lastAheadPosition;//weganAhead.position + back;

            if (backPosition != transform.position && canMove)
            {
                canMove = false;
                RotateWegan(gridDir); // Pass exact direction
                transform.DOMove(backPosition, 0.2f).SetEase(Ease.Linear).OnComplete(() =>
                {
                    canMove = true;
                });
            }

            lastAheadPosition = weganAhead.position; // ✅ Update after calculation
        }
    }
    private void RotateWegan(Vector3 dir)
    {
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);
    }

*/
    private void FollowWagen(){
        if(canMove){
            canMove = false;
            FetchPath();
            
            //SendClearCommand();
            int distance = weganMovePathCurrent.Count;
            if(distance == 0) distance = 1;
            float duration = (float)distance / speed;       // For it move with same velocity
            if(weganMovePathCurrent != null && weganMovePathCurrent.Count >= 1){
                transform.DOPath(ConvertArrayListToVectorArray(weganMovePathCurrent), duration).SetLookAt(0.005f).OnComplete(() => {canMove = true;});
            }else{
                canMove = true;
            }
        }
    }

    
    private void HoldPassedGrids(){
        RaycastHit weganHit;
        if (Physics.Raycast(transform.position, Vector3.down, out weganHit, 5f)) 
        {
            if(weganHit.transform.gameObject.CompareTag("Grid") && weganHit.transform != currentGrid){
                
                if(currentGrid != null) {
                    lastGrid = currentGrid;
                    weganMovePathFromAhead.Add(lastGrid.position);
                }
                currentGrid = weganHit.transform;
            }
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

    private Vector3 PositionRounder(Vector3 _pos){
        return new Vector3(Mathf.Round(_pos.x), _pos.y, Mathf.Round(_pos.z));
    }

    public List<Vector3> GetPath(){
        if(weganMovePathFromAhead != null){
            _weganMovePathFromAhead = new List<Vector3>(weganMovePathFromAhead);
            weganMovePathFromAhead.Clear();
            return _weganMovePathFromAhead;
        } 
        return null;
    }

    public void SendClearCommand(){
        weganAhead.GetComponent<WeganMovement>().ClearPath();
    }

    public void ClearPath(){
        weganMovePathFromAhead.Clear();
    }

    public void FetchPath(){
        weganMovePathCurrent = weganAhead.GetComponent<WeganMovement>().GetPath();
    }



}
