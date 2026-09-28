using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using DestariaMasteries.src.Utils;

namespace DestariaMasteries.src.Systems
{
    public class SocialXpSystem
    {
        private readonly int _tickIntervalMs;
        private readonly float _checkRadiusSq;
        private readonly double _epsilonMovementSq;
        private readonly double _epsilonRotation;
        private readonly int _idleTicksThreshold;
        private readonly int _minPlayersForSocialXp;
        private readonly int _xpAwardedAfterTicks;

        private readonly ICoreServerAPI _sapi;
        private readonly Dictionary<string, PlayerActivityStatus> _activityStatuses = new();
        private readonly long _listenerId;

        public SocialXpSystem(ICoreServerAPI sapi)
        {
            _sapi = sapi;

            SocialSystemConfig cfg = XpRewardEvaluator.Config.SocialSystem;
            _tickIntervalMs = cfg.TickIntervalMs;
            _checkRadiusSq = cfg.CheckRadius * cfg.CheckRadius;
            _epsilonMovementSq = cfg.EpsilonMovement * cfg.EpsilonMovement;
            _epsilonRotation = cfg.EpsilonRotation;
            _idleTicksThreshold = cfg.IdleTicksThreshold;
            _minPlayersForSocialXp = cfg.MinPlayersForSocialXp;
            _xpAwardedAfterTicks = cfg.XpAwardedAfterTicks;

            _listenerId = _sapi.Event.RegisterGameTickListener(OnGameTick, _tickIntervalMs);
            _sapi.Event.PlayerDisconnect += OnDisconnect;
            _sapi.Event.PlayerChat += OnPlayerChat;
        }

        private void OnGameTick(float dt)
        {
            IPlayer[] players = _sapi.World.AllOnlinePlayers;
            if (players.Length < _minPlayersForSocialXp) return;

            foreach (IServerPlayer player in players)
            {
                if (!IsEligible(player)) continue;

                var status = GetOrCreateStatus(player.PlayerUID);
                UpdatePlayerActivity(player, status);

                if (!status.IsActive) continue;

                status.ActiveTicks++;

                if (status.ActiveTicks >= _xpAwardedAfterTicks && CountNearbyActivePlayers(player) >= _minPlayersForSocialXp - 1)
                {
                    XpRewardEvaluator.GrantSocialXp(player, _xpAwardedAfterTicks);
                    status.ActiveTicks = 0;
                }
            }
        }

        private static bool IsEligible(IServerPlayer player)
        {
            if (player.ConnectionState != EnumClientState.Playing) return false;
            var gm = player.WorldData.CurrentGameMode;
            return gm != EnumGameMode.Creative && gm != EnumGameMode.Spectator;
        }

        private void UpdatePlayerActivity(IServerPlayer player, PlayerActivityStatus status)
        {
            Vec3d currentPos = player.Entity.Pos.XYZ;
            float currentYaw = player.Entity.Pos.Yaw;
            float currentPitch = player.Entity.Pos.Pitch;

            if (status.LastPos != null)
            {
                double distSq = currentPos.SquareDistanceTo(status.LastPos);
                double yawDiff = Math.Abs(currentYaw - status.LastYaw);
                double pitchDiff = Math.Abs(currentPitch - status.LastPitch);

                if (distSq < _epsilonMovementSq && yawDiff < _epsilonRotation && pitchDiff < _epsilonRotation)
                {
                    status.IdleTicks++;
                }
                else
                {
                    status.IdleTicks = 0;
                }
            }

            status.LastPos = currentPos;
            status.LastYaw = currentYaw;
            status.LastPitch = currentPitch;

            bool previouslyActive = status.IsActive;
            status.IsActive = status.IdleTicks < _idleTicksThreshold;

            if (!status.IsActive && previouslyActive)
            {
                status.ActiveTicks = 0;
            }
        }

        private int CountNearbyActivePlayers(IServerPlayer sourcePlayer)
        {
            int count = 0;
            Vec3d sourcePos = sourcePlayer.Entity.Pos.XYZ;

            foreach (IServerPlayer otherPlayer in _sapi.World.AllOnlinePlayers)
            {
                if (otherPlayer.PlayerUID == sourcePlayer.PlayerUID || !IsEligible(otherPlayer))
                    continue;

                if (_activityStatuses.TryGetValue(otherPlayer.PlayerUID, out var status) && status.IsActive)
                {
                    if (sourcePos.SquareDistanceTo(otherPlayer.Entity.Pos.XYZ) <= _checkRadiusSq)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private PlayerActivityStatus GetOrCreateStatus(string playerUid)
        {
            if (!_activityStatuses.TryGetValue(playerUid, out var status))
            {
                status = new PlayerActivityStatus();
                _activityStatuses[playerUid] = status;
            }
            return status;
        }

        private void OnDisconnect(IServerPlayer player)
        {
            _activityStatuses.Remove(player.PlayerUID);
        }

        private void OnPlayerChat(IServerPlayer player, int channelId, ref string message, ref string data, BoolRef consumed)
        {
            var status = GetOrCreateStatus(player.PlayerUID);
            status.IsActive = true;
            status.IdleTicks = 0;
        }

        public void Dispose()
        {
            _sapi.Event.UnregisterGameTickListener(_listenerId);
            _sapi.Event.PlayerDisconnect -= OnDisconnect;
            _sapi.Event.PlayerChat -= OnPlayerChat;
        }
    }

    public class PlayerActivityStatus
    {
        public Vec3d? LastPos;
        public float LastYaw;
        public float LastPitch;
        public int IdleTicks;
        public int ActiveTicks;
        public bool IsActive = true;
    }
}