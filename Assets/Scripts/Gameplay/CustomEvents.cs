﻿using System;
using UnityEngine;
using UnityEngine.Events;


[Serializable]
public class OnTowerInit : UnityEvent<TowerData> { }
[Serializable]
public class OnTargetSetEvent : UnityEvent<Transform> { }
