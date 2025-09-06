<%@ Page Language="C#" MasterPageFile="~/VMSMain.master" AutoEventWireup="true" CodeBehind="VisitorSchedule.aspx.cs" Inherits="SCPMS.VisitorSchedule" %>

<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Visitor Schedule Meeting
</asp:Content>

<asp:Content ID="PageTitleContent" ContentPlaceHolderID="PageTitleContent" runat="server">
    Visitor Schedule
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <!-- Link to External Stylesheet for Custom Styling -->
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/intl-tel-input/17.0.19/css/intlTelInput.css" />
    <script src="https://cdnjs.cloudflare.com/ajax/libs/intl-tel-input/17.0.19/js/intlTelInput.min.js"></script>
    <!-- Link to External Stylesheet for Custom Styling -->
    <link rel="stylesheet" type="text/css" href="Css/VisitorSchedule.css">

    <main class="p-6">
        <div class="grid grid-cols-2 gap-6">
            <!-- Meeting Information Section -->
            <section class="bg-white shadow-md rounded-lg p-4">
                <div class="section-header">Meeting Information</div>
                <div class="space-y-4">
                   <div>
                        <label class="block font-medium">Meeting Header <span class="text-red-500">*</span></label>
                        <input id="meetingHeader" name="meetingHeader" type="text" placeholder="Meeting Header" class="w-full p-2 border rounded" required>
                    </div>
                    <div>
                        <label class="block font-medium">Description <span class="text-red-500">*</span></label>
                        <textarea id="description" name="description" placeholder="Description" class="w-full p-2 border rounded" rows="4" required></textarea>
                    </div>
                    <div class="grid grid-cols-2 gap-4">
                        <div>
                            <label class="block font-medium">Meeting Date <span class="text-red-500">*</span></label>
                            <input id="meetingDate" name="meetingDate" type="date" class="w-full p-2 border rounded" required>
                        </div>
                        <div class="grid grid-cols-2 gap-2">
                            <div>
                                <label class="block font-medium">Start Time <span class="text-red-500">*</span></label>
                                <input id="startTime" name="startTime" type="time" class="w-full p-2 border rounded" required>
                            </div>
                            <div>
                                <label class="block font-medium">End Time <span class="text-red-500">*</span></label>
                                <input id="endTime" name="endTime" type="time" class="w-full p-2 border rounded" required>
                            </div>
                        </div>
                    </div>        
                   
                    <div>
                        <label class="block font-medium">Meeting Building <span class="text-red-500">*</span></label>
                        <select id="building" name="building" class="w-full p-2 border rounded" required>
                            <option value="">Select Building</option>
                            <option>Lotus Tower</option>
                        </select>
                    </div>
                    <div class="grid grid-cols-2 gap-4">
                        <div>
                            <label class="block font-medium">Meeting Floor <span class="text-red-500">*</span></label>
                            <select id="floor" name="floor" class="w-full p-2 border rounded" required>
                                <option value="">Select Meeting Floor</option>
                                <option>Basement</option>
                                <option>Ground Floor</option>
                                <option>1st Floor</option>
                                <option>2nd Floor</option>
                                <option>3rd Floor</option>
                            </select>
                        </div>
                    </div>
                    <div>
                        <label class="block font-medium">Meeting Purpose <span class="text-red-500">*</span></label>
                        <select id="purpose" name="purpose" class="w-full p-2 border rounded" required>
                            <option value="">Select Meeting Purpose</option>
                            <option>Payment Collection</option>
                            <option>Business</option>
                            <option>Interview</option>
                            <option>Meeting</option>
                            <option>Investigation</option>
                            <option>Personal</option>
                        </select>
                    </div>
                </div>
            </section>
            
            <!-- Meeting Attendees Section -->
            <section id="attendees-section" class="bg-white shadow-md rounded-lg p-4">
                <div class="section-header">Meeting Attendees</div>

                <!-- Search Section (Always Visible) -->
                <div id="search-section">
                    <div class="flex items-center space-x-2">
                        <input id="search-input" type="text" placeholder="Search Here" class="w-full p-2 border rounded">
                        <select id="search-type" class="p-2 border rounded">
                             <option value="NIC">NIC</option>
                             <option value="Mobile">Mobile No</option>
                             <option value="Email">Email</option>
                        </select>
                        <button type="button" id="search-btn" class="bg-green-500 text-white px-4 py-2 rounded">Search</button>
                    </div>
                </div>

                <!-- Form Section (Initially Visible) -->
                <div id="form-section" class="mt-4">
                    <div class="grid grid-cols-2 gap-4">
                        <div>
                            <label class="block font-medium">Title <span class="text-red-500">*</span></label>
                            <select id="title" name="title" class="w-full p-2 border rounded">
                                <option value="Mr.">Mr.</option>
                                <option value="Ms.">Ms.</option>
                                <option value="Mrs.">Mrs.</option>
                                <option value="Dr.">Dr.</option>
                            </select>
                        </div>
                        <div>
                            <label class="block font-medium">Identification <span class="text-red-500">*</span></label>
                            <input id="identification" name="identification" type="text" placeholder="Identification" class="w-full p-2 border rounded">
                        </div>
                    </div>
                    <div class="grid grid-cols-2 gap-4">
                        <div>
                            <label class="block font-medium">First Name <span class="text-red-500">*</span></label>
                            <input id="first-name" name="first-name" type="text" placeholder="First Name" class="w-full p-2 border rounded">
                        </div>
                        <div>
                            <label class="block font-medium">Last Name <span class="text-red-500">*</span></label>
                            <input id="last-name" name="last-name" type="text" placeholder="Last Name" class="w-full p-2 border rounded">
                        </div>
                    </div>
                    <div class="grid grid-cols-2 gap-4">
                        <div>
                            <label class="block font-medium">Mobile <span class="text-red-500">*</span></label>
                            <input id="mobile-number" name="mobile-number" type="tel" class="w-full p-2 border rounded" placeholder="Enter Mobile Number">
                        </div>
                        <div>
                            <label class="block font-medium">Company</label>
                            <input id="company" name="company" type="text" placeholder="Company Name" class="w-full p-2 border rounded">
                        </div>
                    </div>
                    <div class="grid grid-cols-2 gap-4">
                        <div>
                            <label class="block font-medium">Email</label>
                            <input id="email" name="email" type="email" placeholder="Email" class="w-full p-2 border rounded">
                        </div>
                        <div>
                            <label class="block font-medium">Vehicle No</label>
                            <input id="vehicle-no" name="vehicle-no" type="text" placeholder="Vehicle No" class="w-full p-2 border rounded">
                        </div>
                    </div>
                    <button type="button" id="add-to-queue-btn" class="bg-green-500 text-white px-4 py-2 rounded w-full mt-4">Add to Attendee Queue</button>
                </div>

                <!-- Inserted Attendee Details Table (Initially Hidden) -->
                <div id="attendee-details" class="hidden mt-6">
                    <h3 class="text-lg font-bold">Inserted Attendee Details</h3>
                    <table class="border-table">
                        <thead>
                            <tr>
                                <th>Identification</th>
                                <th>First Name</th>
                                <th>Last Name</th>
                                <th>Mobile</th>
                                <th>Vehicle No</th>
                                <th>Remove</th>
                            </tr>
                        </thead>
                        <tbody id="attendee-table-body">
                             
                            <!-- Inserted Attendee Data will be added here dynamically -->
                        </tbody>
                    </table>
           
                    <!-- Hidden field to store attendees data -->
                    <input type="hidden" id="attendees-data" name="attendees-data" />

                    <!-- Success Message Label -->
                    <asp:Label ID="lblSuccessMessage" runat="server" ForeColor="Green" CssClass="block text-green-600 font-bold mt-2" Visible="true"></asp:Label>
                    <!-- Submit Button -->
                    <asp:Button ID="btnSubmit" runat="server" Text="Submit" OnClick="btnSubmit_Click" CssClass="bg-blue-500 text-white px-4 py-2 rounded mt-4" />
                </div>
            </section>
        </div>
    </main>
<script>
    document.getElementById("search-btn").addEventListener("click", function () {
        let searchValue = document.getElementById("search-input").value.trim();
        let searchType = document.getElementById("search-type").value; // Dropdown selection

        if (searchValue === "") {
            alert("Please enter a search value.");
            return;
        }
        fetch("VisitorSchedule.aspx/GetAttendeeData", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({ searchType: searchType, searchValue: searchValue })
        })
            .then(response => response.json())
            .then(data => {
                if (data.d) { // .d is used because of ASP.NET serialization
                    data = JSON.parse(data.d);
                }
                if (data.error) {
                    alert(data.error);
                } else {
                    document.getElementById("identification").value = data.Identification || "";
                    document.getElementById("first-name").value = data.FirstName || "";
                    document.getElementById("last-name").value = data.LastName || "";
                    document.getElementById("mobile-number").value = data.Mobile || "";
                    document.getElementById("company").value = data.Company || "";
                    document.getElementById("email").value = data.Email || "";
                    document.getElementById("vehicle-no").value = data.VehicleNo || "";
                }
            })
            .catch(error => {
                console.error("Error fetching attendee data:", error);
                alert("Error fetching data. Please try again.");
            });
    });
    function validateNIC() {
        var nicInput = document.getElementById("identification").value;
        var oldNICPattern = /^[0-9]{9}[VXvx]$/;
        var newNICPattern = /^[0-9]{12}$/;

        if (!oldNICPattern.test(nicInput) && !newNICPattern.test(nicInput)) {
            alert("Invalid NIC format! Please enter a valid Sri Lankan NIC (e.g., 123456789V or 200012345678).");
            document.getElementById("identification").value = "";
            return false;
        }
        return true;
    }
    // Attach validation on input field blur event
    document.getElementById("identification").addEventListener("blur", validateNIC);
    // Initialize intl-tel-input plugin on mobile number input
    document.addEventListener('DOMContentLoaded', function () {
        var input = document.querySelector("#mobile-number");
        var iti = window.intlTelInput(input, {
            initialCountry: "lk", // Set initial country code to Sri Lanka (LK)
            separateDialCode: true, // Separate dial code (example: +94) visible in input field
            utilsScript: "https://cdnjs.cloudflare.com/ajax/libs/intl-tel-input/17.0.19/js/utils.js" // Required for formatting
        });
    });
    document.addEventListener("DOMContentLoaded", function () {
        const meetingDate = document.getElementById("meetingDate");
        const startTime = document.getElementById("startTime");
        const endTime = document.getElementById("endTime");
        const submitBtn = document.getElementById("<%= btnSubmit.ClientID %>");

     submitBtn.addEventListener("click", function (event) {
         const today = new Date();
         today.setHours(0, 0, 0, 0);

         const selectedDate = new Date(meetingDate.value);
         selectedDate.setHours(0, 0, 0, 0); // Normalize for accurate date-only comparison

         // 1. Validate Meeting Date
         if (selectedDate < today) {
             alert("Meeting date cannot be in the past!");
             event.preventDefault();
             return;
         }

         const now = new Date();

         // 2. Validate Start Time (only if selected date is today)
         if (startTime.value) {
             const [startHours, startMinutes] = startTime.value.split(":").map(Number);
             const start = new Date();
             start.setHours(startHours, startMinutes, 0, 0);

             if (selectedDate.getTime() === today.getTime() && start < now) {
                 alert("Start Time must be after the current time!");
                 event.preventDefault();
                 return;
             }
         }

         // 3. Validate Start < End Time
         if (startTime.value && endTime.value) {
             const start = new Date(`1970-01-01T${startTime.value}:00`);
             const end = new Date(`1970-01-01T${endTime.value}:00`);

             if (start >= end) {
                 alert("End Time must be later than Start Time!");
                 event.preventDefault();
             }
         }
     });
 });

    // Email validation function with specific domains
    function validateEmail(email) {
        var emailPattern = /^[a-zA-Z0-9._-]+@([a-zA-Z0-9._-]+\.[a-zA-Z]{2,})$/;
        return emailPattern.test(email);
    }
    // Mobile number validation function with Sri Lanka region-specific prefixes
    function validateMobileNumber(mobile) {
        var mobilePattern = /^(070|071|072|074|075|076|077|078)[0-9]{7}$/;
        return mobilePattern.test(mobile);
    }
    // Vehicle number validation function
    function validateVehicleNo(vehicleNo) {
        var vehiclePattern = /^(?:WP|CP|NP|EP|NC|SB|UV|NW|SP|BG|ND|BID)?\s?-?\s?[A-Z]{2,3}?-?\s?\d{4}$/i;
        return vehiclePattern.test(vehicleNo);
    }
    // Array to hold attendee data
    let attendeesList = [];
    document.getElementById("add-to-queue-btn").addEventListener("click", function () {
        let attendee = {
            Identification: document.getElementById("identification").value,
            FirstName: document.getElementById("first-name").value,
            LastName: document.getElementById("last-name").value,
            Mobile: document.getElementById("mobile-number").value,
            Company: document.getElementById("company").value,
            Email: document.getElementById("email").value,
            VehicleNo: document.getElementById("vehicle-no").value
        };
        // Validate required fields
        if (!attendee.FirstName || !attendee.LastName || !attendee.Mobile) {
            alert("Please fill in required fields (First Name, Last Name, Mobile)");
            return;
        }
        // Validate email
        if (!validateEmail(attendee.Email)) {
            alert("Invalid email format! Please enter a valid email address.");
            return;
        }
        // Validate mobile number
        if (!validateMobileNumber(attendee.Mobile)) {
            alert("Invalid mobile number! Please enter a valid Sri Lankan mobile number starting with 070, 071, 072, 074, 075, 076, 077, or 078.");
            return;
        }
        // Validate vehicle number
        if (attendee.VehicleNo && !validateVehicleNo(attendee.VehicleNo)) {
            alert("Invalid vehicle number format! Please enter a valid Sri Lankan vehicle number.");
            return;
        }
        // Add attendee to the array
        attendeesList.push(attendee);
        // Update the hidden field with JSON data
        document.getElementById("attendees-data").value = JSON.stringify(attendeesList);
        // Update the Attendee Table
        let newRow = document.createElement("tr");
        newRow.innerHTML = `
        <td>${attendee.Identification}</td>
        <td>${attendee.FirstName}</td>
        <td>${attendee.LastName}</td>
        <td>${attendee.Mobile}</td>
        <td>${attendee.VehicleNo}</td>
        <td><button type="button" class="remove-btn" style="color: red;">&#10060;</button></td>
    `;
        // Remove attendee from list when clicked
        newRow.querySelector(".remove-btn").addEventListener("click", function () {
            let index = attendeesList.indexOf(attendee);
            if (index !== -1) {
                attendeesList.splice(index, 1);
                document.getElementById("attendees-data").value = JSON.stringify(attendeesList);
            }
            newRow.remove();
        });
        document.getElementById("attendee-table-body").appendChild(newRow);
        document.getElementById("attendee-details").classList.remove("hidden");
        // **Manually Clear Form Fields**
        document.getElementById("identification").value = "";
        document.getElementById("first-name").value = "";
        document.getElementById("last-name").value = "";
        document.getElementById("mobile-number").value = "";
        document.getElementById("company").value = "";
        document.getElementById("email").value = "";
        document.getElementById("vehicle-no").value = "";
        // Reset the dropdown to default
        document.getElementById("title").selectedIndex = 0;
    });
    // Before form submission, store the data into a hidden input
    document.getElementById("btnSubmit").addEventListener("click", function () {
        let attendeeTableBody = document.getElementById("attendee-table-body");
        // Check if the attendee table has at least one entry
        if (attendeeTableBody.children.length === 0) {
            alert("Please add at least one attendee before submitting the meeting.");
            event.preventDefault(); // Prevent form submission
            return;
        }
        document.getElementById("attendees-data").value = JSON.stringify(attendeesList);
    });
    const attendeesSection = document.getElementById('attendees-section');
    const searchBtn = document.getElementById('search-btn');
    // Enable the Attendees Section after search
    searchBtn.addEventListener('click', () => {
        attendeesSection.classList.remove('disabled');
    });
    // Real-time clock display
    function updateClock() {
        const now = new Date();
        const hours = String(now.getHours()).padStart(2, '0');
        const minutes = String(now.getMinutes()).padStart(2, '0');
        const seconds = String(now.getSeconds()).padStart(2, '0');
        document.getElementById('clock').textContent = `${hours}:${minutes}:${seconds}`;
    }
    setInterval(updateClock, 1000);
</script>
</asp:Content>