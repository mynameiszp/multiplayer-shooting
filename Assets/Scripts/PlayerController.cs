using Cinemachine;
using Fusion;
using System;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    //[SerializeField] private SpriteRenderer renderer;
    [SerializeField] private GameObject _cameraPrefab;
    [SerializeField] private float _health;
    [SerializeField] private NetworkMecanimAnimator _animator;
    [SerializeField] private float _speed = 3f;
    [SerializeField] private Rigidbody2D _rigidbody;
    private WeaponManager _weaponManager;
    private CinemachineVirtualCamera _camera;
    private Weapon _weapon;
    [Networked]
    public bool IsDead { get; set; }
    public Action<PlayerController> PlayerDead;

    public override void Spawned()
    {
        if (HasInputAuthority)
        {
            _camera = Instantiate(_cameraPrefab).GetComponent<CinemachineVirtualCamera>();
            //_camera = Runner.Spawn(_cameraPrefab).GetComponent<CinemachineVirtualCamera>();
            _camera.Follow = gameObject.transform;
        }
        //_camera.Follow = gameObject.transform;
        if (!HasStateAuthority) return;
        ConfigureWeapon();
        PlayerDead += Die;
        PlayerDead += DestroyWeapon;
    }

    private void ConfigureWeapon()
    {
        _weaponManager = WeaponManager.Instance;
        WeaponData weaponData = _weaponManager.GetWeapon();
        _weapon = Runner.Spawn(weaponData.prefab).GetComponent<Weapon>();
        _weapon.transform.parent = gameObject.transform;
        _weaponManager.AddWeaponToList(_weapon);
        _weapon.Initialize(_weaponManager.Bullet, weaponData.attackDistance, weaponData.damage, weaponData.attackingEnemyNumber);
    }
    public override void FixedUpdateNetwork()
    {
        var input = GetInput(out NetworkInputData data);
        if (!Runner.IsForward) return;
        if (!IsDead)
        {
            if (data.direction.magnitude > 0)
            {
                data.direction.Normalize();
                _rigidbody.MovePosition(transform.position + Runner.DeltaTime * _speed * (Vector3)data.direction);
                //gameObject.transform.position += Runner.DeltaTime * _speed * (Vector3)data.direction;
                if (data.direction.x > 0)
                {
                    //flip
                }
                else if (data.direction.x < 0)
                {
                    //flip
                }
                Run();
            }
            if (data.aim.magnitude > 0) _weaponManager.StartPlayersAttack(_weapon, data.aim); //not now
            if (data.direction.magnitude == 0) Idle();
        }
        else
        {
            _animator.Animator.SetBool(AnimationVariables.PLAYER_DEAD_ANIMATION, true);
        }
    }
    public void Run()
    {
        _animator.Animator.SetBool(AnimationVariables.PLAYER_RUN_ANIMATION, true);
    }
    public void Idle()
    {
        _animator.Animator.SetBool(AnimationVariables.PLAYER_RUN_ANIMATION, false);
    }
    public void Die(PlayerController player)
    {
        IsDead = true;
    }
    public void TakeHealth(float damage)
    {
        _health -= damage;
        if (_health <= 0)
        {
            PlayerDead?.Invoke(this);
        }
    }
    private void DestroyWeapon(PlayerController player)
    {
        Runner.Despawn(_weapon.GetComponent<NetworkObject>());
    }
}
