using Fusion;
using Fusion.Addons.Physics;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject.SpaceFighter;

public class Enemy : NetworkBehaviour
{
    public float Damage { get; set; }
    public float AttackFrequency { get; set; }
    public float Speed { get; set; }
    public float Health { get; set; }
    [Networked] private TickTimer _attackTimer { get; set; }
    [SerializeField] private NetworkRigidbody2D _networkRigidbody;
    [SerializeField] private Rigidbody2D _rigidbody;
    private PlayerController _targetPlayer;
    private List<PlayerController> _players;

    public Action<Enemy> OnDestroy;
    public void Init(List<PlayerController> players)
    {
        _players = players;
        foreach (var player in _players)
        {
            player.PlayerDead += ForgetPlayer;
        }
    }
    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;
        Move();
    }

    public void TakeHealth(float damage)
    {
        Health -= damage;
    }

    public float GetHealth()
    {
        return Health;
    }

    private void Attack(PlayerController player)
    {
        player.TakeHealth(Damage);
    }

    private void Move()
    {
        float distance = 0f;
        _targetPlayer = null;
        foreach (var player in _players)
        {
            if (Vector3.Distance(gameObject.transform.position, player.transform.position) >= distance)
            {
                _targetPlayer = player;
            }
        }
        if (_targetPlayer != null)
        {
            Vector3 direction = (_targetPlayer.transform.position - gameObject.transform.position).normalized;
            Vector3 newPosition = transform.position + (Speed * Runner.DeltaTime * direction);
            _rigidbody.MovePosition(newPosition);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent(out PlayerController player))
        {
            Attack(player);
            _attackTimer = TickTimer.CreateFromSeconds(Runner, AttackFrequency);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (_attackTimer.Expired(Runner))
        {
            if (collision.gameObject.TryGetComponent(out PlayerController player))
            {
                Attack(player);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent(out PlayerController player))
        {
            _attackTimer = TickTimer.None;
        }
    }

    private void ForgetPlayer(PlayerController player)
    {
        _players.Remove(player);
    }

    public void Destroy()
    {
        Runner.Despawn(GetComponent<NetworkObject>());
        OnDestroy?.Invoke(this);
    }
}
