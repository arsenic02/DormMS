import { useState } from 'react';
import { rankFreshmen, rankOlderStudents } from '../../api/ranking';
import styles from './DormManagerRanking.module.css';

const AVERAGE_SALARY = 121650;

function getSocialBonus(incomePerMember) {
  return incomePerMember > 0 && incomePerMember < AVERAGE_SALARY * 0.5 ? 1 : 0;
}

function getEspbBonus(espb) {
  if (espb > 240) return 2;
  if (espb >= 120) return 1;
  return 0;
}

function formatNumber(value) {
  return Number(value).toLocaleString('sr-RS', { maximumFractionDigits: 2 });
}

function StatusBadge({ status }) {
  if (status === 'Assigned') {
    return <span className={styles.badgeAssigned}>Primljen</span>;
  }
  if (status === 'Waitlisted') {
    return <span className={styles.badgeWaitlisted}>Lista čekanja</span>;
  }
  return <span className={styles.badgeNeutral}>{status}</span>;
}

function OlderStudentsTable({ students }) {
  return (
    <table className={styles.table}>
      <thead>
        <tr>
          <th>Ime i prezime</th>
          <th>Broj indeksa</th>
          <th>Prosek (fakultet)</th>
          <th>ESPB</th>
          <th>Godina studija</th>
          <th>Socijalni bonus</th>
          <th>Dodatak</th>
          <th>Ukupno</th>
          <th>Status</th>
        </tr>
      </thead>
      <tbody>
        {students.map((s) => (
          <tr key={s.studentId} className={s.status === 'Assigned' ? styles.rowAssigned : undefined}>
            <td>{s.studentName} {s.studentSurname}</td>
            <td className={styles.muted}>{s.studentIDNumber}</td>
            <td className={styles.numeric}>{formatNumber(s.averageGrade)}</td>
            <td className={styles.numeric}>{s.espb}</td>
            <td className={styles.numeric}>{s.yearsOfStudy}</td>
            <td className={styles.numeric}>{getSocialBonus(s.incomePerMember)}</td>
            <td className={styles.numeric}>{getEspbBonus(s.espb)}</td>
            <td className={styles.numericBold}>{formatNumber(s.calculatedPoints)}</td>
            <td><StatusBadge status={s.status} /></td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}

function FreshmenTable({ students }) {
  return (
    <table className={styles.table}>
      <thead>
        <tr>
          <th>Ime i prezime</th>
          <th>Broj indeksa</th>
          <th>Prosek (srednja škola)</th>
          <th>Socijalni bonus</th>
          <th>Ukupno</th>
          <th>Status</th>
        </tr>
      </thead>
      <tbody>
        {students.map((s) => (
          <tr key={s.studentId} className={s.status === 'Assigned' ? styles.rowAssigned : undefined}>
            <td>{s.studentName} {s.studentSurname}</td>
            <td className={styles.muted}>{s.studentIDNumber}</td>
            <td className={styles.numeric}>{formatNumber(s.highSchoolAverageGrade)}</td>
            <td className={styles.numeric}>{getSocialBonus(s.incomePerMember)}</td>
            <td className={styles.numericBold}>{formatNumber(s.calculatedPoints)}</td>
            <td><StatusBadge status={s.status} /></td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}

function FacultyAccordion({ facultyName, students, isFreshmanView, isOpen, onToggle }) {
  const assignedCount = students.filter((s) => s.status === 'Assigned').length;

  return (
    <div className={styles.card}>
      <button className={styles.cardHeader} onClick={onToggle} type="button">
        <div className={styles.cardHeaderLeft}>
          <i className={`ti ti-chevron-right ${styles.chevron} ${isOpen ? styles.chevronOpen : ''}`} aria-hidden="true" />
          <span className={styles.facultyName}>{facultyName}</span>
        </div>
        <span className={styles.summary}>
          {students.length} prijavljenih &middot; {assignedCount} primljeno
        </span>
      </button>

      {isOpen && (
        <div className={styles.cardBody}>
          {isFreshmanView ? (
            <FreshmenTable students={students} />
          ) : (
            <OlderStudentsTable students={students} />
          )}
        </div>
      )}
    </div>
  );
}

export default function DormManagerRanking({ competitionId, isFreshmanView }) {
  const [rankingData, setRankingData] = useState(null);
  const [openFaculties, setOpenFaculties] = useState({});
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  async function handleRunRanking() {
    setLoading(true);
    setError(null);

    try {
      const result = isFreshmanView
        ? await rankFreshmen(competitionId)
        : await rankOlderStudents(competitionId);

      setRankingData(result);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }

  function toggleFaculty(facultyName) {
    setOpenFaculties((prev) => ({ ...prev, [facultyName]: !prev[facultyName] }));
  }

  const faculties = rankingData ? Object.entries(rankingData.rankingsByFaculty) : [];

  return (
    <div className={styles.container}>
      <div className={styles.toolbar}>
        <h2 className={styles.title}>
          {isFreshmanView ? 'Rangiranje brucoša' : 'Rangiranje starijih studenata'}
        </h2>
        <button className={styles.runButton} onClick={handleRunRanking} disabled={loading} type="button">
          {loading ? 'Rangiranje u toku...' : 'Pokreni rangiranje'}
        </button>
      </div>

      {error && <p className={styles.error}>{error}</p>}

      {rankingData && (
        <p className={styles.totalInfo}>
          Ukupno studenata: {rankingData.totalStudents}
        </p>
      )}

      {faculties
        .sort(([, a], [, b]) => b.length - a.length)
        .map(([facultyName, students]) => (
          <FacultyAccordion
            key={facultyName}
            facultyName={facultyName}
            students={students}
            isFreshmanView={isFreshmanView}
            isOpen={!!openFaculties[facultyName]}
            onToggle={() => toggleFaculty(facultyName)}
          />
        ))}
    </div>
  );
}

// import { useState } from 'react';
// import { rankFreshmen, rankOlderStudents } from '../../api/ranking';
// import styles from './DormManagerRanking.module.css';

// const AVERAGE_SALARY = 121650;

// function getSocialBonus(incomePerMember) {
//   return incomePerMember > 0 && incomePerMember < AVERAGE_SALARY * 0.5 ? 1 : 0;
// }

// function getEspbBonus(espb) {
//   if (espb > 240) return 2;
//   if (espb >= 120) return 1;
//   return 0;
// }

// function formatNumber(value) {
//   return Number(value).toLocaleString('sr-RS', { maximumFractionDigits: 2 });
// }

// function StatusBadge({ status }) {
//   if (status === 'Assigned') {
//     return <span className={styles.badgeAssigned}>Primljen</span>;
//   }
//   if (status === 'Waitlisted') {
//     return <span className={styles.badgeWaitlisted}>Lista čekanja</span>;
//   }
//   return <span className={styles.badgeNeutral}>{status}</span>;
// }

// function OlderStudentsTable({ students }) {
//   return (
//     <table className={styles.table}>
//       <thead>
//         <tr>
//           <th>Ime i prezime</th>
//           <th>Broj indeksa</th>
//           <th>Prosek (fakultet)</th>
//           <th>ESPB</th>
//           <th>Godina studija</th>
//           <th>Socijalni bonus</th>
//           <th>Dodatak</th>
//           <th>Ukupno</th>
//           <th>Status</th>
//         </tr>
//       </thead>
//       <tbody>
//         {students.map((s) => (
//           <tr key={s.studentId} className={s.status === 'Assigned' ? styles.rowAssigned : undefined}>
//             <td>{s.studentName} {s.studentSurname}</td>
//             <td className={styles.muted}>{s.studentIDNumber}</td>
//             <td className={styles.numeric}>{formatNumber(s.averageGrade)}</td>
//             <td className={styles.numeric}>{s.espb}</td>
//             <td className={styles.numeric}>{s.yearsOfStudy}</td>
//             <td className={styles.numeric}>{getSocialBonus(s.incomePerMember)}</td>
//             <td className={styles.numeric}>{getEspbBonus(s.espb)}</td>
//             <td className={styles.numericBold}>{formatNumber(s.calculatedPoints)}</td>
//             <td><StatusBadge status={s.status} /></td>
//           </tr>
//         ))}
//       </tbody>
//     </table>
//   );
// }

// function FreshmenTable({ students }) {
//   return (
//     <table className={styles.table}>
//       <thead>
//         <tr>
//           <th>Ime i prezime</th>
//           <th>Broj indeksa</th>
//           <th>Prosek (srednja škola)</th>
//           <th>Socijalni bonus</th>
//           <th>Ukupno</th>
//           <th>Status</th>
//         </tr>
//       </thead>
//       <tbody>
//         {students.map((s) => (
//           <tr key={s.studentId} className={s.status === 'Assigned' ? styles.rowAssigned : undefined}>
//             <td>{s.studentName} {s.studentSurname}</td>
//             <td className={styles.muted}>{s.studentIDNumber}</td>
//             <td className={styles.numeric}>{formatNumber(s.highSchoolAverageGrade)}</td>
//             <td className={styles.numeric}>{getSocialBonus(s.incomePerMember)}</td>
//             <td className={styles.numericBold}>{formatNumber(s.calculatedPoints)}</td>
//             <td><StatusBadge status={s.status} /></td>
//           </tr>
//         ))}
//       </tbody>
//     </table>
//   );
// }

// function FacultyAccordion({ facultyName, students, isFreshmanView, isOpen, onToggle }) {
//   const assignedCount = students.filter((s) => s.status === 'Assigned').length;

//   return (
//     <div className={styles.card}>
//       <button className={styles.cardHeader} onClick={onToggle} type="button">
//         <div className={styles.cardHeaderLeft}>
//           <i className={`ti ti-chevron-right ${styles.chevron} ${isOpen ? styles.chevronOpen : ''}`} aria-hidden="true" />
//           <span className={styles.facultyName}>{facultyName}</span>
//         </div>
//         <span className={styles.summary}>
//           {students.length} prijavljenih &middot; {assignedCount} primljeno
//         </span>
//       </button>

//       {isOpen && (
//         <div className={styles.cardBody}>
//           {isFreshmanView ? (
//             <FreshmenTable students={students} />
//           ) : (
//             <OlderStudentsTable students={students} />
//           )}
//         </div>
//       )}
//     </div>
//   );
// }

// export default function DormManagerRanking({ competitionId, isFreshmanView }) {
//   const [rankingData, setRankingData] = useState(null);
//   const [openFaculties, setOpenFaculties] = useState({});
//   const [loading, setLoading] = useState(false);
//   const [error, setError] = useState(null);

//   async function handleRunRanking() {
//     console.log('competitionId koji se salje:', competitionId)
//     setLoading(true);
//     setError(null);

//     try {
//       const result = isFreshmanView
//         ? await rankFreshmen(competitionId)
//         : await rankOlderStudents(competitionId);

//       setRankingData(result);
//     } catch (err) {
//       setError(err.message);
//     } finally {
//       setLoading(false);
//     }
//   }

//   function toggleFaculty(facultyName) {
//     setOpenFaculties((prev) => ({ ...prev, [facultyName]: !prev[facultyName] }));
//   }

//   const faculties = rankingData ? Object.entries(rankingData.rankingsByFaculty) : [];

//   return (
//     <div className={styles.container}>
//       <div className={styles.toolbar}>
//         <h2 className={styles.title}>
//           {isFreshmanView ? 'Rangiranje brucoša' : 'Rangiranje starijih studenata'}
//         </h2>
//         <button className={styles.runButton} onClick={handleRunRanking} disabled={loading} type="button">
//           {loading ? 'Rangiranje u toku...' : 'Pokreni rangiranje'}
//         </button>
//       </div>

//       {error && <p className={styles.error}>{error}</p>}

//       {rankingData && (
//         <p className={styles.totalInfo}>
//           Ukupno studenata: {rankingData.totalStudents}
//         </p>
//       )}

//       {faculties
//         .sort(([, a], [, b]) => b.length - a.length)
//         .map(([facultyName, students]) => (
//           <FacultyAccordion
//             key={facultyName}
//             facultyName={facultyName}
//             students={students}
//             isFreshmanView={isFreshmanView}
//             isOpen={!!openFaculties[facultyName]}
//             onToggle={() => toggleFaculty(facultyName)}
//           />
//         ))}
//     </div>
//   );
// }
