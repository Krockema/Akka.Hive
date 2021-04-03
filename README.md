# Akka.Hive
This is an extension of the Akka actor framework that allows switching the environment between simulation with virtual time, simulation with real time or integration of real endpoints.

# Background
Based on my Master thesis with a kinda slow solution for a .Net based DES System and similar behave to the Aktor pattern the idea came across to give Akka.Net a try to build a superfast Aktor based discrete event simulation system. This is the outcome to this idea since i did not found any .Net based solutions that satisfies my requirements. Your welcome to improve and further develop the system. 

# Features
- [x] Basis DES with Example
- [x] Configuratble Simulation Interrupts
- [x] Custom Message Monitoring to take actions on specific ocurances

# ToDo's
- [ ] More / Better Unit Tests
- [ ] May implement a diffrent Distributed Clock like
  * https://en.wikipedia.org/wiki/Vector_clock or 
  * https://en.wikipedia.org/wiki/Lamport_timestamps
