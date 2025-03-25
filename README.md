# Rendezvous

![Demo of Rendezvous](docs/rendezvous-matches.png)

Rendezvous is dating application which can help members find matches, communicate and build connections.

**Table of Contents**
- [Functionalities](#functionalities)
  * [Members](#members)
  * [Moderators](#moderators)
  * [Admins](#admins)
- [Run Application](#run-application)

## Functionalities

Application provides functionalities based on roles.

### Members

- **Profile and Photos** - Members can manage their profile, and upload photos which are reviewed by admins or moderators.
- **Matches** - Members can browse and view other members' profiles.
- **Likes** - Members can like other members' profiles and see who likes them.
- **Messaging** - Members can send and receive messages to/from other members.

### Moderators

- **Photo Management** - Moderators can approve or reject member photos.

### Admins

- **User Management** - Admins can modify member roles.
- **Photo Management** - Admins can approve or reject member photos.

## Run Application

**Requirements**:
- Docker

1. Create `.env` file (see: [.env.example](.env.example)).
2. Set up [appsettings.json](Rendezvous.API/appsettings.json).
3. Run the following commane: `docker compose up -d`.

Application should be available at [localhost:8080](http://localhost:8080).
