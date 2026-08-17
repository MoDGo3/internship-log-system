export interface UserRecord {
  userID: number;
  username: string;
  email: string;
  role: string;
  firstName: string;
  lastName: string;
  isActive: boolean;
}

export interface CompanyRecord {
  companyID: number;
  companyName: string;
  address: string;
  contactPerson: string;
  contactEmail: string;
  contactPhone: string;
}

export interface InternshipRecord {
  internshipID: number;
  studentID: number;
  companyID: number;
  mentorID: number;
  advisorID: number;
  startDate: string;
  endDate: string;
  status: string;
}
