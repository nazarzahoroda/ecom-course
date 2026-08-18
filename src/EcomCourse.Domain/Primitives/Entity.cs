using System;
using System.Collections.Generic;
using System.Text;

namespace EcomCourse.Domain.Primitives
{
    public abstract class Entity<TId>
    {

        public TId Id { get; protected set; }

        private readonly List<IDomainEvent> _domainEvents = new();

        public IReadOnlyCollection<IDomainEvent> GetDomainEvents() => _domainEvents.ToList();

        protected void RaiseDomainEvents(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }


        protected Entity(TId id)
        {
            Id = id;
        }

    }
}
