using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "AIBehaviours/BotMussuna")]
public class BotMussurana : AIBehaviour
{
    private enum State
    {
        IDLE, EATING, RUNNING, WANDER
    }

    private Collider2D[] _collidersList;

    private List<GameObject> _opponentsList = new List<GameObject>();
    private List<GameObject> _orbsList = new List<GameObject>();
    private State _currentState = State.WANDER;
    private Transform _targetObject;

    public override void Init(GameObject own, SnakeMovement ownMove)
    {
        base.Init(own, ownMove);
        ownerMovement.StartCoroutine(UpdateDirEveryXSeconds(timeChangeDir));
    }

    //seria interessante ter um controlador com o colisor que define o mundo pra poder gerar pontos dentro desse colisor
    public override void Execute()
    {
        //MoveForward();
        GetNearbyObjects();
        BotController();
        //COMENDO
        //FUGINDO
        //ANDANDO ALEATORIAMENTE
        //ATACAR
    }

    //ia basica, move, muda de direcao e move
    private void MoveForward()
    {
        //MouseRotationSnake();
        //owner.transform.position = Vector2.MoveTowards(owner.transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition), ownerMovement.speed * Time.deltaTime);
    }

    private void GetNearbyObjects()
    {
        _collidersList = Physics2D.OverlapCircleAll(owner.transform.position, 10);

        _opponentsList.Clear();
        _orbsList.Clear();

        foreach (Collider2D collider in _collidersList)
        {
            if(collider.tag == "Body")
            {
                if(Vector3.Distance(collider.gameObject.transform.position, owner.transform.position) < 1f)
                {
                    _opponentsList.Add(collider.gameObject);
                }
            }
            else if(collider.tag == "Orb")
            {
                _orbsList.Add(collider.gameObject);
            }
        }
    }

    private void BotController()
    {
        owner.transform.position = Vector2.MoveTowards(owner.transform.position, _targetObject.transform.position, ownerMovement.speed * Time.deltaTime);

        if(_currentState == State.WANDER)
        {
            if(_orbsList.Count > 0)
            {
                GetOrb();
            }
        }
        else if(_currentState == State.EATING)
        {
            VerifyTarget();
        }
    }

    private void GetOrb()
    {
        GameObject nearestOrb = null;

        foreach(GameObject orb in _orbsList)
        {
            if(nearestOrb == null)
            {
                nearestOrb = orb;
            }
            else
            {
                float lastDist = Vector3.Distance(nearestOrb.gameObject.transform.position, owner.gameObject.transform.position);
                float newDist = Vector3.Distance(orb.gameObject.transform.position, owner.gameObject.transform.position);

                if(newDist < lastDist)
                {
                    nearestOrb = orb;
                }
            }
        }

        direction = nearestOrb.gameObject.transform.position - owner.transform.position;
        direction.z = 0.0f;
        _targetObject = nearestOrb.transform;
        _currentState = State.EATING;
    }

    private void VerifyTarget()
    {
        if(_targetObject == null)
        {
            _currentState = State.WANDER;
        }
        else
        {
            owner.transform.position = Vector2.MoveTowards(owner.transform.position, _targetObject.transform.position, ownerMovement.speed * Time.deltaTime);
        }
    }

    private IEnumerator UpdateDirEveryXSeconds(float x)
    {
        yield return new WaitForSeconds(x);

        /*ownerMovement.StopCoroutine(UpdateDirEveryXSeconds(x));
        randomPoint = new Vector3(
                Random.Range(
                    Random.Range(owner.transform.position.x - 10, owner.transform.position.x - 5),
                    Random.Range(owner.transform.position.x + 5, owner.transform.position.x + 10)
                ),
                Random.Range(
                    Random.Range(owner.transform.position.y - 10, owner.transform.position.y - 5),
                    Random.Range(owner.transform.position.y + 5, owner.transform.position.y + 10)
                ),
                0
            );
        direction = randomPoint - owner.transform.position;
        direction.z = 0.0f;

        ownerMovement.StartCoroutine(UpdateDirEveryXSeconds(x));*/
    }
}
