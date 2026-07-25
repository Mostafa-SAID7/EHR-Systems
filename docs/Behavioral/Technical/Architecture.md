# Microservices Architecture Principles

---

## 🎯 Service Boundary Guidelines

1. **Domain-Driven Design (DDD)**: Group services around business bounded contexts (e.g., Coding Engine, Claims Submission, Vendor/Hospital Management).
2. **Database per Service**: Prevent direct cross-database queries; enforce API/event communication.
3. **Stateless Services**: Scale horizontally behind API Gateways and Load Balancers.
