SLIDE 12: LIVE DEMO
Slide Title: Let's Build a Loan Application Form

Content:

Start with an empty form component
Add controlled inputs one at a time
Add validation with onBlur
Add submission handling
Build a quantity selector component
[INSTRUCTOR: Switch to VS Code]

Speaker Notes: "Let's build this live. I'll start with an empty component and add inputs one at a time so you can see the controlled pattern build up."

[See LIVE CODING SCRIPT below]

Timing: 12 minutes

LIVE CODING SCRIPT (12 minutes)
Step 1: Basic Controlled Input (2 min)
Start with LoanForm.tsx:

import { useState } from "react";

function LoanForm() {
const [applicantName, setApplicantName] = useState("");

return (
<form>
<label>
Applicant Name
<input
type="text"
value={applicantName}
onChange={(e) => setApplicantName(e.target.value)}
/>
</label>
<p>You typed: {applicantName}</p>
</form>
);
}

export default LoanForm;
Narrate: "Simplest possible controlled input. State holds the value, input displays it, onChange updates it. See the paragraph below — it updates with every keystroke. That's the power of controlled components: React always knows the current value."

[Type in the field, show the paragraph updating live]

Step 2: Consolidate to Form Object (2 min)
Refactor to use a single state object:

type LoanFormData = {
applicantName: string;
email: string;
loanAmount: number | "";
loanType: string;
};

function LoanForm() {
const [formData, setFormData] = useState<LoanFormData>({
applicantName: "",
email: "",
loanAmount: "",
loanType: "",
});

const handleChange = (
e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>,
) => {
const { name, value, type } = e.target;
setFormData((prev) => ({
...prev,
[name]: type === "number" ? (value === "" ? "" : Number(value)) : value,
}));
};

return (
<form>
<label>
Applicant Name
<input
          type="text"
          name="applicantName"
          value={formData.applicantName}
          onChange={handleChange}
        />
</label>

      <label>
        Email
        <input
          type="email"
          name="email"
          value={formData.email}
          onChange={handleChange}
        />
      </label>

      <label>
        Loan Amount ($)
        <input
          type="number"
          name="loanAmount"
          value={formData.loanAmount}
          onChange={handleChange}
          min={1000}
          max={500000}
        />
      </label>

      <label>
        Loan Type
        <select name="loanType" value={formData.loanType} onChange={handleChange}>
          <option value="">Select a type...</option>
          <option value="personal">Personal</option>
          <option value="auto">Auto</option>
          <option value="home">Home</option>
        </select>
      </label>
    </form>

);
}
Narrate: "Now one state object holds all form data. The generic handleChange uses the input's name attribute — notice every input has a name that matches the key in our state. The computed property [name]: value updates the right field. Number inputs get special treatment — we convert the string to a number.

This scales. Add a fifth field? Add a key to the type, add an initial value, add an input with a matching name. The handler just works."

Step 3: Add Validation (3 min)
Add error state, touched state, and onBlur:

const [errors, setErrors] = useState<Record<string, string | undefined>>({});
const [touched, setTouched] = useState<Set<string>>(new Set());

const validateField = (name: string, value: string | number | ""): string | undefined => {
switch (name) {
case "applicantName":
if (typeof value === "string" && value.trim().length < 2)
return "Name must be at least 2 characters";
break;
case "email":
if (typeof value === "string" && value.length > 0 && !value.includes("@"))
return "Enter a valid email";
break;
case "loanAmount":
if (value === "") return "Loan amount is required";
if (typeof value === "number" && value < 1000) return "Minimum $1,000";
if (typeof value === "number" && value > 500000) return "Maximum $500,000";
break;
case "loanType":
if (!value) return "Select a loan type";
break;
}
return undefined;
};

const handleBlur = (e: React.FocusEvent<HTMLInputElement | HTMLSelectElement>) => {
const { name } = e.target;
setTouched(prev => new Set(prev).add(name));
const error = validateField(name, formData[name as keyof LoanFormData]);
setErrors(prev => ({ ...prev, [name]: error }));
};
Add to each input:

<input
type="text"
name="applicantName"
value={formData.applicantName}
onChange={handleChange}
onBlur={handleBlur}
aria-invalid={touched.has("applicantName") && !!errors.applicantName}
/>
{touched.has("applicantName") && errors.applicantName && (
<span className="error" role="alert">{errors.applicantName}</span>
)}
Narrate: "I add onBlur to every input. When the user leaves a field, we validate it and add the field name to the touched set. The error only shows if the field has been touched AND has an error.

Watch — I'll click into the name field, type one letter, then tab out. Error appears: 'Name must be at least 2 characters'. I add another letter, tab out again — error clears. That's the on-blur pattern.

The aria-invalid attribute is important — it tells screen readers the field has a problem."

[Demo: type one character, tab away, show error; type more, tab away, error clears]

Step 4: Handle Submission (2 min)
const handleSubmit = (e: React.FormEvent) => {
e.preventDefault();

// Validate all
const newErrors: Record<string, string | undefined> = {};
for (const [key, value] of Object.entries(formData)) {
newErrors[key] = validateField(key, value);
}

const hasErrors = Object.values(newErrors).some(Boolean);
setErrors(newErrors);
setTouched(new Set(Object.keys(formData)));

if (hasErrors) return;

alert(`Submitted: ${JSON.stringify(formData, null, 2)}`);
};
Narrate: "On submit: prevent default first — always. Then validate every field. If any errors exist, show them all and return. If clean, submit. I'm using alert() for now — in a real app this would be a fetch() to your API."

[Demo: click submit with empty fields — all errors appear at once]

Step 5: Build a Quantity Selector (3 min)
Create QuantitySelector.tsx:

type QuantitySelectorProps = {
value: number;
onChange: (newValue: number) => void;
min?: number;
max?: number;
};

function QuantitySelector({ value, onChange, min = 1, max = 99 }: QuantitySelectorProps) {
const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
const raw = parseInt(e.target.value, 10);
if (isNaN(raw)) return;
onChange(Math.min(max, Math.max(min, raw)));
};

return (
<div style={{ display: "flex", alignItems: "center", gap: "8px" }}>
<button
type="button"
onClick={() => onChange(Math.max(min, value - 1))}
disabled={value <= min}
aria-label="Decrease quantity" >
−
</button>
<input
type="number"
value={value}
onChange={handleChange}
min={min}
max={max}
style={{ width: "60px", textAlign: "center" }}
aria-label="Quantity"
/>
<button
type="button"
onClick={() => onChange(Math.min(max, value + 1))}
disabled={value >= max}
aria-label="Increase quantity" > +
</button>
</div>
);
}
Add a test usage to the form:

const [testQuantity, setTestQuantity] = useState(1);

// Inside the form JSX:

<div>
  <p>Test quantity: {testQuantity}</p>
  <QuantitySelector value={testQuantity} onChange={setTestQuantity} />
</div>
Narrate: "Last piece — the quantity selector. This is the component you'll reuse in M4 for every cart item. Plus and minus buttons, a number input in the middle, clamped to min and max.

Notice: type='button' on the +/− so they don't accidentally submit a form. disabled when at boundaries. aria-label for accessibility.

And it's a fully controlled component — the parent owns the value and the onChange. You can drop this into any component and it just works."

[Demo: click +/−, type a number directly, show clamping at boundaries]

BACKUP PLAN: Pre-written completed version in completed/ directory.

SLIDE 13: Key Takeaways from Demo
Slide Title: What You Just Saw

Content:

Controlled inputs = value + onChange, always
Generic handler with [name]: value scales to any number of fields
Validation on blur with a touched set = good UX
Submission = preventDefault() + validate all + submit or show errors
Quantity selector = reusable controlled component for M4 cart
Speaker Notes: "Five patterns. You'll use all five on Friday and in M4. The quantity selector in particular — build it once, use it for every cart item."

Timing: 1 minute
