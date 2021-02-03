﻿using CQRS_Write_Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CQRS_Write_Domain
{
    public class AggregateRoot<T> : IAggregateRoot<T>
    {
        private List<IEvent> eventChanges = new List<IEvent>();
        public T Id { get; protected set; }

        public int Version { get; protected set; }

        public void ApplyChange(IEvent @event, bool isNew = true)
        {
            var method = this.GetType().GetMethod("Apply", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { @event.GetType()}, null);
            
            if (method != null)
                method.Invoke(this, new object[] { @event });

            if (isNew)
            {
                @event.Version = this.eventChanges.Count() + 1;
                @event.Timestamp = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();
                this.eventChanges.Add(@event);
            }
                
        }

        public object GetId()
        {
            return Id;
        }

        public IEnumerable<IEvent> GetUncommitedChanges()
        {
            return this.eventChanges;
        }

        public void LoadFromHistory(IEnumerable<IEvent> history)
        {
            foreach (var @event in history)
            {
                this.ApplyChange(@event, false);
            }
        }

        public void MarkChangesAsCommited()
        {
            this.eventChanges.Clear();
        }
    }
}
