import { parseMermaidToExcalidraw } from "@excalidraw/mermaid-to-excalidraw";
import fs from "fs";

const mermaidCode = `classDiagram
    class User {
        +int UserID
        +string Email
        +string PasswordHash
        +string Role
        +bool IsActive
        +DateTime CreatedAt
    }
    class Student {
        +int StudentID
        +string FirstName
        +string LastName
        +string RegistrationNumber
        +string Department
        +int Semester
        +string Phone
    }
    class Admin {
        +int AdminID
        +string FirstName
        +string LastName
        +string Phone
    }
    class Society {
        +int SocietyID
        +string Name
        +string Description
        +string Category
        +int? HeadStudentID
        +string Status
        +DateTime CreatedAt
        +int? ApprovedByAdminID
    }
    class Event {
        +int EventID
        +int SocietyID
        +string Title
        +DateTime EventDate
        +int? MaxParticipants
        +string Status
    }
    class SocietyTask {
        +int TaskID
        +int SocietyID
        +string Title
        +int AssignedToStudentID
        +int AssignedByStudentID
        +string Status
    }
    class Announcement {
        +int AnnouncementID
        +int SocietyID
        +string Title
        +string Content
    }
    class MembershipRequest {
        +int RequestID
        +int StudentID
        +int SocietyID
        +string Status
    }
    class SocietyMember {
        +int MemberID
        +int StudentID
        +int SocietyID
        +string Role
    }
    class EventRegistration {
        +int RegistrationID
        +int EventID
        +int StudentID
        +string TicketCode
    }

    User <|-- Student
    User <|-- Admin
    Society "1" --> "*" Event
    Society "1" --> "*" SocietyTask
    Society "1" --> "*" Announcement
    Society "1" --> "*" MembershipRequest
    Student "1" --> "*" MembershipRequest
    Society "1" --> "*" SocietyMember
    Student "1" --> "*" SocietyMember
    Event "1" --> "*" EventRegistration
    Student "1" --> "*" EventRegistration
`;

async function run() {
    try {
        const { elements, files } = await parseMermaidToExcalidraw(mermaidCode, {
            fontSize: 20
        });
        const excalidrawFile = {
            type: "excalidraw",
            version: 2,
            source: "https://excalidraw.com",
            elements: elements,
            appState: { viewBackgroundColor: "#ffffff" },
            files: files || {}
        };
        fs.writeFileSync("ClassDiagram.excalidraw", JSON.stringify(excalidrawFile, null, 2));
        console.log("Excalidraw diagram generated successfully.");
    } catch (e) {
        console.error("Error generating diagram:", e);
    }
}

run();
