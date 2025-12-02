# System Specification (English Translation)

## Project Goal
The platform helps real estate (NT) brokers publish and manage property listings while giving potential buyers a seamless way to browse listing information, see open house dates, view seller contacts, and book private tours. Buyer identity verification gates access to sensitive data and reduces spam or data scraping of seller information.

## Operating Principle
The system consists of two pillars:
1. **Web user interface** – used by every role (unregistered visitors, registered buyers, brokers, and administrators).
2. **API layer** – exposes all business capabilities that the UI consumes.

## Functional Requirements by Role

### Unregistered Visitor (Buyer)
- Search property listings without limits.
- View listing details (excluding seller contact information).
- See scheduled open-house days for a listing.
- Sign in to the system.
- Register as a regular user (buyer).
- Register as a real-estate broker.

### Registered Buyer
- Sign out of the system.
- Complete identity verification.
- View seller contact details once verified.
- Book a private property viewing.
- Review and manage their booked private viewings.

### Real-Estate Broker
- Sign out of the system.
- Update personal contact information.
- See their buildings inventory.
- Create, edit, and delete buildings.
- See apartments tied to a building.
- Create, edit, and delete apartments for a building.
- Upload and delete apartment photos.
- Mark which photos are public in listings.
- Review their listings.
- Create, edit, and delete listings for apartments.
- Define their availability slots for private tours (and disable the feature if needed).
- Review private viewing requests tied to their listings.
- Approve or reject incoming private viewing requests.

### Administrator
- Review all system users.
- Block abusive users.
- Approve broker registrations.
- Delete problematic listings.

## Additional Notes
- Buyer identity verification protects sensitive seller information from bots and reduces unsolicited contact.
- When brokers create listings, building and apartment data auto-populate; they only enter price and optional extra information.
- Private viewing scheduling relies on broker-defined availability slots to minimize direct phone coordination.
