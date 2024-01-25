using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class DependencyContainer : MonoInstaller
{
    [SerializeField] private WeaponManager _weaponManager;
    [SerializeField] private PlayerAnimation _playerAnimation;
    public override void InstallBindings()
    {
        Container.Bind<WeaponManager>().FromInstance(_weaponManager);
        Container.Bind<PlayerAnimation>().FromInstance(_playerAnimation);
    }
}
